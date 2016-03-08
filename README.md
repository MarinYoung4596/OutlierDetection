# OutlierDetection

##Overview
This is an program for outlier Detection.

This project consist of 4 sub-projects, named,

 1. [Generate Random Databse][1]
 2. [Outlier Detection][2]
 3. [Outlier Analysis][3]
 4. [Python Implementation][4]

We **generate a random database** for unit test to get the performance of these algorithms, *Angle-Based Outlier Detection* (ABOD), *Density-Based Outlier Detection* (LOF), and *Distance-Based Outlier Detection* (DBOD).

We **implement** all these algorithms in Visual Studio 2013 based on C# programming language. And use **Outlier Analysis** project to analyze the result.

##Techniques Details
1. Distance Based Approach
![DBOD][5]
2. Density Based Method
![LOF_1][6]
![LOF_2][7]
3. Angle Based Algorithm
![ABOD_1][8]
![ABOD_2][9]
![ABOF][10]
##Statistical Results
1. Precision
![FastABOD_Precision_Dimension][11]
![LBABOD_Precision_Dimension][12]
![Precision_Dimension_N500][13]
![Precision_Dimension_N1000][14]
2. Dimension
![Time_D_ABOD_LBABOD][15]
![Time_D_FastABOD][16]
![Time_D_LOF][17]
3. CPU Time
![LBABOD_Time_D][18]
 

##References

To better understand the program in folder [OutlierDetection], please read the following papers:

 1. [Angle-based Outlier Detection in High-Dimensional Data][19]
 2. [Outlier Detection Techniques][20](p.s. You may find this documents is helpful to you. enjoy :-) )


##Notes
Any more questions, please feel free to contact me :-). marinyoung(at)163.com


  [1]: https://github.com/MarinYoung4596/OutlierDetection/tree/master/OutlierDetection/GenerateRandomDatabase
  [2]: https://github.com/MarinYoung4596/OutlierDetection/tree/master/OutlierDetection/OutlierDetection
  [3]: https://github.com/MarinYoung4596/OutlierDetection/tree/master/OutlierDetection/OutlierAnalysis
  [4]: https://github.com/MarinYoung4596/OutlierDetection/tree/master/OutlierDetection/Python%20Implementation
  [5]: https://github.com/MarinYoung4596/OutlierDetection/blob/master/OutlierDetection/OutlierAnalysis/StatisticalResults/DBOD_1.PNG
  [6]: https://github.com/MarinYoung4596/OutlierDetection/blob/master/OutlierDetection/OutlierAnalysis/StatisticalResults/LOF_1.PNG
  [7]: https://github.com/MarinYoung4596/OutlierDetection/blob/master/OutlierDetection/OutlierAnalysis/StatisticalResults/LOF_2.PNG
  [8]: https://github.com/MarinYoung4596/OutlierDetection/blob/master/OutlierDetection/OutlierAnalysis/StatisticalResults/ABOD_1.PNG
  [9]: https://github.com/MarinYoung4596/OutlierDetection/blob/master/OutlierDetection/OutlierAnalysis/StatisticalResults/ABOD_2.PNG
  [10]: https://github.com/MarinYoung4596/OutlierDetection/blob/master/OutlierDetection/OutlierAnalysis/StatisticalResults/ABOF.PNG
  [11]: https://github.com/MarinYoung4596/OutlierDetection/blob/master/OutlierDetection/OutlierAnalysis/StatisticalResults/FastABOD_Precision_Dimension.png
  [12]: https://github.com/MarinYoung4596/OutlierDetection/blob/master/OutlierDetection/OutlierAnalysis/StatisticalResults/LBABOD_Precision_Dimension.png
  [13]: https://github.com/MarinYoung4596/OutlierDetection/blob/master/OutlierDetection/OutlierAnalysis/StatisticalResults/Precision_Dimension_N500.png
  [14]: https://github.com/MarinYoung4596/OutlierDetection/blob/master/OutlierDetection/OutlierAnalysis/StatisticalResults/Precision_Dimension_N1000.png
  [15]: https://github.com/MarinYoung4596/OutlierDetection/blob/master/OutlierDetection/OutlierAnalysis/StatisticalResults/Time_D_ABOD_LBABOD.png
  [16]: https://github.com/MarinYoung4596/OutlierDetection/blob/master/OutlierDetection/OutlierAnalysis/StatisticalResults/Time_D_FastABOD.png
  [17]: https://github.com/MarinYoung4596/OutlierDetection/blob/master/OutlierDetection/OutlierAnalysis/StatisticalResults/Time_D_LOF.png
  [18]: https://github.com/MarinYoung4596/OutlierDetection/blob/master/OutlierDetection/OutlierAnalysis/StatisticalResults/LBABOD_Time_D.png
  [19]: http://dl.acm.org/citation.cfm?id=1401946
  [20]: https://www.siam.org/meetings/sdm10/tutorial3.pdf