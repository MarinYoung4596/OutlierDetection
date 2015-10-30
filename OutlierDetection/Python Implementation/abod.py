# !/usr/bin/env python
# -*- coding: utf-8 -*-

# ABOD FastABOD and LB-ABOD algorithm implementation
# Code by Marin Young
# Created at 2014/4/25

import csv
import sys
import numpy as np
import math
import codecs
import time
import datetime
from operator import itemgetter
import logging

# import mail


####################logging####################
logger = logging.getLogger('logger')
logger.setLevel(logging.INFO)

logging.basicConfig(level=logging.DEBUG,
					format='%(asctime)s %(name)-12s %(levelname)-8s %(message)s',
					datefme='%m-%d %H:%M:%S',
					filename='logging.log',
					filemode='a+')

# ch: console handler
ch = logging.StreamHandler()
ch.setLevel(logging.INFO)

formatter = logging.Formatter('%(asctime)-12s %(levelname)-8s %(message)s')
ch.setFormatter(formatter)

logging.getLogger('').addHandler(ch)


def logPrint(logger, level, info):
	if cmp(level, 'DEBUG') == 0:
		logger.debug(info)
	elif cmp(level, 'INFO') == 0:
		logger.info(info)
	elif cmp(level, 'WARNING') == 0:
		logger.warning(info)
	elif cmp(level, 'ERROR') == 0:
		logger.ERRORor(info)
	elif cmp(level, 'CRITICAL') == 0:
		logger.critical(info)


####################compute RunTime####################
def getCurrTime():
	currTime = time.strftime('%y-%m-%d %H:%M:%S', time.localtime(time.time()))
	return currTime


def computeRunTime(startTime, endTime):
	start = datetime.datetime.strptime(startTime, '%y-%m-%d %H:%M:%S')
	start_sec_float = time.mktime(start.timetuple())
	
	end = datetime.datetime.strptime(endTime, '%y-%m-%d %H:%M:%S')
	end_sec_float = time.mktime(end.timetuple())

	return end_sec_float - start_sec_float


####################load file####################
def sampling(srcFile, dstFile, N):
	"""
	Sampleing top @N rows from @srcFile and write into @dstFile
	"""
	info = 'Sampling: get the top %d points from source file...' % N
	logPrint(logger, 'INFO', info)
	data = []
	i = 0
	with open(srcFile, 'r') as read:
		reader = csv.reader(read)
		try:
			for line in reader:
				if i < N:
					data.append(line)
					i += 1
		except csv.Error as e:
			sys.exit('FILE %s, LINE %d: %s' %(read, reader.line_num, e))
	
	with open(dstFile, 'w') as write:
		writer = csv.writer(write)
		writer.writerows(data)


def loadCsvIntoTable(fSrc, nRows):
	"""
	load csv file into memory
	"""
	result = [list() for i in range(nRows)]
	i = 0
	with open(fSrc, 'r') as f:
		while True:
			line = f.readline()
			if not line:
				break
			if line.strip() == '':
				continue
			result[i % nRows].append(line.strip())
			i += 1
	return result


def loadCsvIntoTable_2(fSrc):
	"""
	load csv file @fSrc into memory
	"""
	logPrint(logger, 'INFO', 'Loading source file...')
	coord = []
	with open(fSrc, 'r') as f:
		for line in f:
			if line.startswith(codecs.BOM_UTF8):
				line = line[3:]				# drop '\xef\xbb\xbf'
			line = line[:len(line) - 2]		# drop '\r\n'
			info = line.split(',')
			coord.append(info)
	return coord


def writeFile(data, fSrc):
	"""
	save @data into @fSrc
	"""
	logPrint(logger, 'INFO', 'Saving data...')
	with open(fSrc, 'w') as f:
		outcsv = csv.writer(f, delimiter=',')
		outcsv.writerow(data)


####################pre-processing####################
def normalization(rowDataList):
	"""
	standardize all the attributes
	use Z-Score method
	"""
	npDataList = [float(s) for s in rowDataList]
	npDataList = np.array(npDataList)
	means = npDataList.mean()
	stdDev = npDataList.std()			#standard deviation
	normalizedList = []
	for x in npDataList:
		y = (x - means) / stdDev
		normalizedList.append(y)
	return normalizedList


def attributesSelection(data):
	"""
	feature selection
	"""
	startTime = getCurrTime()
	data = np.array(data)
	(row, column) = data.shape
	coord = []
	tmp = []
	i = 0
	for i in range(column - 3):			# Drop [ID, ClusterID, Metoid]
		if i == 1 or i == 2:			# Drop [timestampArrive, nRadioOn(NULL)]
			continue
		tmp = [data[row][i + 3] for row in xrange(len(data))] # fetch each column of data
		tmp = normalization(tmp)		# normalization
		coord.append(tmp)
	result = np.transpose(coord)		# transpose
	info = 'Feature selection...\tRunTime %d' % computeRunTime(startTime, getCurrTime())
	logPrint(logger, 'INFO', info)
	return result


def dictSortByValue(d, reverse=False):
	"""
	sort the given dictionary by value
	"""
	return sorted(d.iteritems(), key = itemgetter(1), reverse=False)
	

def euclDistance(A, B):	
	"""
	calculate the euclidean metric of @A and @B
	A = (x1, x2, ..., xn)  are initialized
	"""
	if len(A) != len(B):
		sys.exit('ERROR\teuclDistance\tpoint A and B not in the same space')
	vector_AB = A - B
	tmp = vector_AB ** 2
	tmp = tmp.sum()
	distance = tmp ** 0.5
	return distance


def computeDistMatrix(pointsList):
	"""
	compute distance of each pair of points and stored in distTable
	"""
	startTime = getCurrTime()
	pointsList = np.array(pointsList)
	(nrow, ncolumn) = pointsList.shape
	distTable = [[] for i in range(nrow)]  # define a row * row array
	i = 0
	for i in range(nrow):
		j = 0
		for j in range(i + 1):
			if i == j:
				distTable[i].append(0.0)
				continue
			mold_ij = euclDistance(pointsList[i], pointsList[j])
			distTable[i].append(mold_ij)
	info = 'Calculate distance matrix\tRunTime %d' % computeRunTime(startTime, getCurrTime())
	logPrint(logger, 'INFO', info)
	return distTable


def angleBAC(A, B, C, AB, AC):				# AB AC mold
	"""
	calculate <AB, AC>
	"""
	vector_AB = B - A						# vector_AB = (x1, x2, ..., xn)
	vector_AC = C - A	
	mul = vector_AB * vector_AC				# mul = (x1y1, x2y2, ..., xnyn)
	dotProduct = mul.sum()					# dotProduct = x1y1 + x2y2 + ... + xnyn
	try:
		cos_AB_AC_ = dotProduct / (AB * AC) # cos<AB, AC>
	except ZeroDivisionError:
		sys.exit('ERROR\tangleBAC\tdistance can not be zero!')
	if math.fabs(cos_AB_AC_) > 1: 
		print 'A\n', A
		print 'B\n', B
		print 'C\n', C
		print 'AB = %f, AC = %f' % (AB, AC)
		print 'AB * AC = ', dotProduct
		print '|AB| * |AC| = ', AB * AC
		sys.exit('ERROR\tangleBAC\tmath domain ERROR, |cos<AB, AC>| <= 1')
	angle = float(math.acos(cos_AB_AC_))	# <AB, AC> = arccos(cos<AB, AC>)
	return angle


####################ABOD algorithm implement####################
def ABOF(pointsList, A, index, distTable):
	"""
	calculate the ABOF of A = (x1, x2, ..., xn)
	"""
	pointsList = np.array(pointsList)
	i = 0
	varList = []
	for i in range(len(pointsList)):
		if i == index:						# ensure A != B
			continue
		B = pointsList[i]
		if index < i:
			AB = distTable[i][index]
		else:								# index > i
			AB = distTable[index][i]

		j = 0
		for j in range(i + 1):
			if j == index or j == i:		# ensure C != A && B != C
				continue
			C = pointsList[j]
			if index < j:
				AC = distTable[j][index]
			else:							# index > j
				AC = distTable[index][j]

			angle_BAC = angleBAC(A, B, C, AB, AC)

			# compute each element of variance list
			try:
				tmp = angle_BAC / float(math.pow(AB * AC, 2))
			except ZeroDivisionError:
				sys.exit('ERROR\tABOF\tfloat division by zero!')
			varList.append(tmp)
	variance = np.var(varList)
	return variance


def ABOD(fSrc, resultListPath, topK):
	"""
	ABOD algorithm implementation
	"""
	startTime = getCurrTime()
	data = loadCsvIntoTable_2(fSrc)
	pointsList = attributesSelection(data)
	distTable = computeDistMatrix(pointsList)
	DictABOF = {}
	i = 0
	for A in pointsList:
		start_time = time.clock()
		ABOF_A = ABOF(pointsList, A, i, distTable)
#		if (i + 1) % 100 == 0:
#			print '100 points, START %s\tEND %s' % (startTime, getCurrTime()) 
		DictABOF.setdefault(i + 1, ABOF_A) # in reality, points ID start with 1
		i += 1

	logPrint(logger, 'INFO', 'Sorting ABOF list...')
	outlierList = dictSortByValue(DictABOF)
	outlier = []
	j = 0
	for k,v in outlierList:
		if j < topK:
			outlier.append(k)
			j += 1

	writeFile(outlier, resultListPath)
	endTime = getCurrTime()
	info = 'ABOD\tSTART %s\tEND %s\tRunTime %d' % (startTime, endTime, computeRunTime(startTime, endTime))
	logPrint(logger, 'INFO', info)


####################Fast ABOD algorithm implemement####################
def getkNNIndex(pointsList, A, index, kNN, distTable):
	"""
	get k nearest neighbor of point A
	"""
	distDictOfA = {}
	i = 0
	for i in range(len(pointsList)):
		if i == index:
			continue
		elif i > index:
			AB = distTable[i][index]
		else:					# i < index
			AB = distTable[index][i]
		distDictOfA[i] = AB
	sortedDistList = dictSortByValue(distDictOfA)
	kNNIndex = []				# k nearest neighbor point list
	j = 0
	for k,v in sortedDistList:
		if j < kNN:
			kNNIndex.append(k)
			j += 1
	return kNNIndex


def ApproxABOF(pointsList, A, indexA, distTable, kNN):
	"""
	calculating the approximate ABOF of point A
	"""
	pointsList = np.array(pointsList)
	distTable = np.array(distTable)
	# get the index of k nearest neighbor points of point A
	kNNIndex = getkNNIndex(pointsList, A, indexA, kNN, distTable)
	# compute Approximate ABOF
	varList = []
	i = 0
	for i in range(len(kNNIndex)):
		indexB = kNNIndex[i]
		B = pointsList[indexB]
		if indexA > indexB:
			AB = distTable[indexA][indexB]
		elif indexA < indexB:
			AB = distTable[indexB][indexA]

		j = 0
		for j in range(i + 1):
			if i == j:
				continue
			indexC = kNNIndex[j]
			C = pointsList[indexC]
			if indexA > indexC:
				AC = distTable[indexA][indexC]
			elif indexA < indexC:
				AC = distTable[indexC][indexA]

			angle_BAC = angleBAC(A, B, C, AB, AC)
			try:
				tmp = angle_BAC / float(math.pow(AB * AC, 2))
			except ZeroDivisionError:
				sys.exit('ERROR\tApproxABOF\tfloat division by zero!')
			varList.append(tmp)
	variance = np.var(varList)
	return variance


def FastABOD(fSrc, fFastABOD, kNN, topK):
	"""
	Fast ABOD algorithm implementation
	"""
	startTime = getCurrTime()
	data = loadCsvIntoTable_2(fSrc)
	pointsList = attributesSelection(data)
	distTable = computeDistMatrix(pointsList)
	DictApprABOF = {}
	i = 0
	for A in pointsList:
		apprABOF = ApproxABOF(pointsList, A, i, distTable, kNN)
		DictApprABOF.setdefault(i + 1, apprABOF)
		i += 1

	outlierList = dictSortByValue(DictApprABOF)

	outlier = []
	j = 0
	for k,v in outlierList:
		if j < topK:
			outlier.append(k)
			j += 1
	writeFile(outlier, fFastABOD)
	endTime = getCurrTime()
	info = 'FastABOD\tSTART %s\tEND %s\tRunTime %d' % (startTime, endTime, computeRunTime(startTime, endTime))
	logPrint(logger, 'INFO', info)


####################LB-ABOD algorithm implement####################
def Denominator(D, A, index, distTable, power):
	"""
	calculate the denominator of ABOF, ApproxABOF and LB_ABOF
	"""
	i = 0
	deno = 0
	for i in range(len(D)):
		if i == index:
			continue
		B = D[i]
		if index > i:
			AB = distTable[index][i]
		else:
			AB = distTable[i][index]
		j = 0
		for j in range(i + 1):
			if j == i or j == index:
				continue
			C = D[j]
			if index > j:
				AC = distTable[index][j]
			else:
				AC = distTable[j][index]
			try:
				tmp = 1 / (AB * AC)
			except ZeroDivisionError:
				sys.exit('ERROR\tDenominator\tdistance can not be zero!')
			deno += math.pow(tmp, power)
	return deno


def R2(D, Nk, A, index, distTable):
	"""
	calculate R2 in LB-ABOF
	"""
	minuend = Denominator(D, A, index, distTable, 3)
	subtranend = Denominator(Nk, A, index, distTable, 3)
	return (minuend - subtranend)


def LB_ABOF(pointsList, A, index, kNN, distTable):
	"""
	calculate the LB-ABOF of point A
	"""
	kNNIndex = getkNNIndex(pointsList, A, index, kNN, distTable)
	Nk = []
	minuend_molecule = 0.0
	subtrahend_molecule = 0.0
	i = 0
	for i in range(len(kNNIndex)):
		indexB = kNNIndex[i]
		if index == indexB:
			continue
		B = pointsList[indexB]
		Nk.append(B)
		if index > indexB:
			AB = distTable[index][indexB]
		else:	# index < indexB
			AB = distTable[indexB][index]
		j = 0
		for j in range(i + 1):
			if i == j:
				continue
			indexC = kNNIndex[j]
			if index == indexC:
				continue
			C = pointsList[indexC] 
			if index > indexC:
				AC = distTable[index][indexC]
			else:
				AC = distTable[indexC][index]
			
			angle_BAC = angleBAC(A, B, C, AB, AC)
			try:
				tmp = angle_BAC / (math.pow(AB * AC, 3))   # <AB, AC> / (|AB|^3 * |AC|^3)
			except ZeroDivisionError:
				sys.exit('ERROR\tLB_ABOF\tdistance can not be zero!')
			minuend_molecule += math.pow(tmp, 2)
			subtrahend_molecule += tmp
	r2 = R2(pointsList, Nk, A, index, distTable)
	deno = Denominator(pointsList, A, index, distTable, 1)
	try:
		minuend = minuend_molecule / deno
		subtrahend = math.pow( ((subtrahend_molecule + r2) / deno), 2)
	except ZeroDivisionERRORor:
		sys.exit('ERROR\tLB_ABOF\tdeominator can not be zero!')
	LBABOF_A = minuend - subtrahend
	return LBABOF_A


def LB_ABOD(fSrc, fResultPath, kNN, topK):
	"""
	LB-ABOD algorithm implementation
	"""
	startTime = getCurrTime()
	data = loadCsvIntoTable_2(fSrc)
	pointsList = attributesSelection(data)
	distTable = computeDistMatrix(pointsList)
	# (step 2) 	Compute LB-ABOF for each point A ∈ D.
    # (step 3) 	Organize the database objects in a candidate list ordered ascendingly  
    #			w.r.t. their assigned LB-ABOF.
	DictLBABOF = {}
	i = 0
	for A in pointsList:
		LBABOF_A = LB_ABOF(pointsList, A, i, kNN, distTable)
		DictLBABOF.setdefault(i, LBABOF_A)
		i += 1
	candidateList = dictSortByValue(DictLBABOF) # sorted LB-ABOF list ascendingly
	
	print 'candidateList:\ni\tLB-ABOF\n'
	for (k, v) in candidateList:
		print '%d\t%f' % (k, v)

    # (step 4) 	Determine the exact ABOF for the first @topK objects in the candidate
    #			list, Remove them from the candidate list and insert into the current
    #			result list.
	resultList = {}								# result list: outlier
	j = 0
	for j in range(topK):
		(k, v) = candidateList.pop(0)
		A = pointsList[k]
		ABOFValueOfA = ABOF(pointsList, A, k, distTable)
		resultList[k] = ABOFValueOfA
    # (step 6)	if the largest ABOF in the result list < the smallest approximated 
    #			ABOF in the candidate list, terminate; else, proceed with (step 5).
    # (step 5)	Remove and examine the next best candidate C from the candidate list
    #			and determine the exact ABOF, if the ABOF of C < the largest ABOF
    #			of anobject A in the result list, remove A from the result list and
    #			insert C into the result list.
	resultList = dictSortByValue(resultList)
	MIN_LBABOF = candidateList[0][1]
	MAX_ABOF = resultList[topK - 1][1]
	while MAX_ABOF > MIN_LBABOF:
		(Ck, Cv) = candidateList.pop(0)
		C = pointsList[Ck]
		ABOF_C = ABOF(pointsList, C, Ck, distTable)
		if ABOF_C < MAX_ABOF: 
			del resultList[len(resultList) - 1]
			resultList.append((Ck, ABOF_C))
			resultList = sorted(resultList, key=lambda t: t[1], reverse=False)
			
			MIN_LBABOF = candidateList[0][1]
			MAX_ABOF = resultList[topK - 1][1]
	
	print 'resultList:\ni\tABOF\n'
	for (k, v) in resultList:
		print '%d\t%f' % (k, v)

	# saving result...
	outlier = []
	for k,v in resultList:
		outlier.append(k + 1)   # In reality, outlier ID start with 1; in pointsList, it start with 0
	writeFile(outlier, fResultPath)
	endTime = getCurrTime()
	info = 'LB-ABOD\tSTART %s\tEND %s\tRunTime %d' % (startTime, endTime, computeRunTime(startTime, endTime))
	logPrint(logger, 'INFO', info)


def main():
	N = 200
	kNN = 100
	topK = 50
	
	fSrc = 'data/StatusData.csv'

	fSampledFile = 'data/StatusData%d.csv' % N
	fDist = 'data/DistanceMatrix%d.csv' % N
	
	# file name format: FastABOD_@N_@kNN_@topK_@currTime.csv
	currTime = str(time.strftime('%m%d%H%M%S'))
	fABOD = 'data/ABOD_%d__%d_%s.csv' % (N, topK, currTime)
	fFastABOD = 'data/FastABOD_%d_%d_%d_%s.csv' % (N, kNN, topK, currTime)
	fLBABOD = 'data/LBABOD_%d_%d_%d_%s.csv' % (N, kNN, topK, currTime)

	info = 'N = %d\tkNN = %d\ttopK = %d' % (N, kNN, topK)
	logPrint(logger, 'INFO', info)

	sampling(fSrc, fSampledFile, N)
#	FastABOD(fSampledFile, fFastABOD, kNN, topK)
	LB_ABOD(fSampledFile, fLBABOD, kNN, topK)
#	ABOD(fSampledFile, fABOD, topK)

#	data = loadCsvIntoTable_2(fSrc)
#	pointsList = attributesSelection(data)
#	distTable = computeDistMatrix(pointsList)
#	writeFile(distTable, fdist)


if __name__ == '__main__':
	main()
#	mail.sendEMail()
