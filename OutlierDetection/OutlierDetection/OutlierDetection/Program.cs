/* *******************************************************************
 * Created:     2014/5/20
 * Last Modified:2014.5.28
 * Project:     Outlier Detection
 * Filename: 	Program.cs
 * File ext:	cs
 * Author:		Marin Young
 * Purpose:	ABOD/FastABOD/LB-ABOD/LOF algorithm implement   
 * ******************************************************************/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using OutlierDetection.SQLServerDataAccess;
using OutlierDetection.SysUtil;

namespace OutlierDetection
{
    class Program
    {
        public static int LENGTH = 500;

        public static int DIMENSION = 80;

        public static string tableName = String.Format("TestTable_{0}", DIMENSION);

        private static double[,] distTable = new double[LENGTH, LENGTH];
        private static SQLServerDataProvider data = new SQLServerDataProvider(tableName, LENGTH);


        #region Program Auxiliary Functions

        /// <summary>
        /// calculate run time
        /// </summary>
        /// <param name="dateBegin"></param>
        /// <param name="dateEnd"></param>
        /// <returns>total run time</returns>
        private static string calcRunTime(DateTime dateBegin, DateTime dateEnd)
        {
            TimeSpan tsBegin = new TimeSpan(dateBegin.Ticks);
            TimeSpan tsEnd = new TimeSpan(dateEnd.Ticks);
            TimeSpan tsDiff = tsEnd.Subtract(tsBegin).Duration();
            return tsDiff.TotalSeconds.ToString();
        }


        /// <summary>
        /// save all the DPoint into @path
        /// </summary>
        /// <param name="outlier"></param>
        /// <param name="fpath"></param>
        private static void saveDPoint(List<DPoint> outlier, string fpath)
        {
            StreamWriter sw = File.AppendText(fpath);
            sw.WriteLine("======================================");
            sw.WriteLine("ID\tValue");
            foreach (DPoint x in outlier)
                sw.WriteLine(x.ID + "\t" + x.Value);
            sw.Flush();
            sw.Close();
        }


        /// <summary>
        /// save @topK outliers
        /// </summary>
        /// <param name="outlier"></param>
        /// <param name="topK"></param>
        /// <param name="fpath"></param>
        private static void saveOutlier(List<DPoint> outlier, int topK, string fpath)
        {
            StreamWriter sw = File.AppendText(fpath);
            sw.WriteLine("==============outlier=================");
            DPoint X;
            for (int i = 0; i < topK; i++)
            {
                X = outlier[i];
                sw.Write(X.ID + "\t");
            }
            sw.WriteLine();
            sw.Flush();
            sw.Close();
        }


        /// <summary>
        /// printf @str into @fpath
        /// </summary>
        /// <param name="str"></param>
        /// <param name="fpath"></param>
        private static void strfprintf(string str, string fpath)
        {
            StreamWriter sw = File.AppendText(fpath);
            sw.WriteLine(str);
            sw.Flush();
            sw.Close();
        }


        /// <summary>
        /// add index of database to DStatusPacket @src
        /// </summary>
        /// <param name="index"></param>
        /// <param name="src"></param>
        /// <returns></returns>
        private static DStatus addIndexToPacket(int index, DStatusPacket src)
        {
            DStatus dst = new DStatus();
            dst.Index = index;
            dst.ID = src.ID;
            dst.Attributes = src.Attributes;
            return dst;
        }


        /// <summary>
        /// drop index form packet (convert DStatus to DStatusPacket)
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        private static DStatusPacket dropIndexFromPacket(DStatus src)
        {
            DStatusPacket dst = new DStatusPacket();
            dst.ID = src.ID;
            dst.Attributes = src.Attributes;
            return dst;
        }

        #endregion


        #region Functionality

        /// <summary>
        /// calculate euclidean distTableance of point @A and @B
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns>|AB|</returns>
        private static double euclDistance(DStatusPacket A, DStatusPacket B)
        {
            DStatusPacket tmp = new DStatusPacket();
            tmp = A - B;        // vector BA = A - B, we have reloaded the operator - in class DStatusPacket
            double result = 0.0;
            for (int i = 0; i < A.Attributes.Count(); i++)
            {
                result += Math.Pow(tmp.Attributes[i], 2);
            }
            return Math.Sqrt(result);
        }


        /// <summary>
        /// calculate distTableance matrix of each pair of points 
        /// </summary>
        private static void calcDistMatrix()
        {
            DStatusPacket A, B;
            for (int i = 0; i < LENGTH; i++)
            {
                A = data.getDataByID(i);
                for (int j = i + 1; j < LENGTH; j++)
                {
                    B = data.getDataByID(j);
                    // Upper triangular matrix
                    distTable[i, j] = euclDistance(A, B);
                }
            }
        }


        /// <summary>
        /// calculate dot product of vector @A and @B
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns>A * B</returns>
        private static double calcDotProduct(DStatusPacket vectorAB, DStatusPacket vectorAC)
        {
            double result = 0.0;
            for (int i = 0; i < vectorAB.Attributes.Count(); i++)
            {
                result += vectorAB.Attributes[i] * vectorAC.Attributes[i];
            }
            return result;
        }


        /// <summary>
        /// calculate angle BAC(<AB, AC>)
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <param name="C"></param>
        /// <returns><AB, AC></returns>
        private static double calcAngleBAC(DStatusPacket A, DStatusPacket B, DStatusPacket C, double AB, double AC)
        {
            DStatusPacket vector_AB = B - A;
            DStatusPacket vector_AC = C - A;
            double dotProduct, cos_BAC = 0.0, angle;

            dotProduct = calcDotProduct(vector_AB, vector_AC);
            try
            {
                cos_BAC = dotProduct / (AB * AC);
            }
            catch (DivideByZeroException e)
            {
                Console.WriteLine("Overflow Exception:" + e.Message);
            }
            if (Math.Abs(cos_BAC) > 1)
            {
                Console.WriteLine("DotProduct: " + dotProduct + "\tAB: " + AB + "\tAC: " + AC);
                Console.WriteLine("Math Domain Error: |cos<AB, AC>| <= 1");
            }
            angle = Math.Acos(cos_BAC);
            return angle;
        }


        /// <summary>
        /// calculate the variance of list @v
        /// </summary>
        /// <param name="v"></param>
        /// <returns>var</returns>
        private static double calcVariance(double[] v)
        {
            if (v.Length == 0)
            {
                Console.WriteLine("Error: v.Length can't be 0!");
                return 0;
            }

            double sum = 0.0, mean, var = 0.0;
            foreach (double x in v)
                sum += x;
            mean = sum / v.Length;      // ensure v.Length != 0
            foreach (double x in v)
                var += Math.Pow((x - mean), 2);
            var = var / v.Length;
            return var;
        }

        #endregion


        #region ABOD: Angle-based

        /// <summary>
        /// calculate ABOF of point @A
        /// </summary>
        /// <param name="D">neighbour of point A</param>
        /// <param name="A"></param>
        /// <param name="index"></param>
        /// <returns>ABOF(A)</returns>
        private static double ABOF(List<DStatus> D, DStatusPacket A, int index)
        {
            DStatusPacket B, C;
            int indexB, indexC;
            double AB, AC;      // AB = |AB|, AC = |AC|
            double angle_BAC, tmp = 0.0, variance;
            double dotProductOfABandAC;     // AB * AC
            /*
             * For ABOD, D include all the points, except for the point @A, the size 
             * of @tmpList is (D.Count() - 1) * (D.Count() - 2) / 2 ;
             * However, for Fast ABOD, @D is the k nearest neighbor of point @A, when 
             * index >= Nk.Count(), there's no need to except @A, therefore, the size 
             * of @tmpList is D.Count * (D.Count - 1) / 2.
             */
            double[] tmpList = new double[D.Count() * (D.Count() - 1) / 2];
            int k = 0;

            for (int i = 0; i < D.Count(); i++)
            {
                indexB = D[i].Index;
                if (indexB == index)
                    continue;
                B = dropIndexFromPacket(D[i]);
                if (indexB < index)
                    AB = distTable[indexB, index];
                else
                    AB = distTable[index, indexB];

                for (int j = i + 1; j < D.Count(); j++)
                {
                    indexC = D[j].Index;
                    if (indexC == index || indexC == indexB)
                        continue;
                    C = dropIndexFromPacket(D[j]);
                    if (indexC < index)
                        AC = distTable[indexC, index];
                    else
                        AC = distTable[index, indexC];

                    dotProductOfABandAC = calcDotProduct(B - A, C - A);

                    /* //debug
                    angle_BAC = calcAngleBAC(A, B, C, AB, AC);
                    if (AB == 0 || AC == 0 || angle_BAC == 0)
                        Console.WriteLine("indexA: {0}\tindexB: {1}\tindexC: {2}\tAB: {3}\tAC: {4}\tangle_BAC = {5}", index, indexB, indexC, AB, AC, angle_BAC);
                    */

                    try
                    {
                        tmp = dotProductOfABandAC / Math.Pow(AB * AC, 2);
                    }
                    catch (DivideByZeroException e)
                    {
                        Console.WriteLine("Overflow Exception:" + e.Message);
                    }
                    tmpList[k++] = tmp;
                }
            }
            variance = calcVariance(tmpList);
            return variance;
        }


        /// <summary>
        /// ABOD algorithm implement
        /// </summary>
        /// <param name="topK">topK points are outlier</param>
        public static void ABOD(int topK, String fpath, DateTime timeStart)
        {
            List<DPoint> ABOFList = new List<DPoint>(LENGTH);
            List<DStatus> D = new List<DStatus>(LENGTH);
            DStatusPacket tmpPacket;
            DPoint tmpPoint = new DPoint();
            DStatusPacket A;
            double ABOF_A;

            for (int i = 0; i < LENGTH; i++)
            {
                tmpPacket = data.getDataByID(i);
                D.Add(addIndexToPacket(i, tmpPacket));
            }
            for (int j = 0; j < D.Count(); j++)
            {
                A = D[j];
                ABOF_A = ABOF(D, A, j);

                tmpPoint = new DPoint(A.ID, ABOF_A);
                ABOFList.Add(tmpPoint);
            }
            ABOFList.Sort();       // Sort ABOF list by ABOF value

            DateTime timeEnd = DateTime.Now;

            saveDPoint(ABOFList, fpath);
            saveOutlier(ABOFList, topK, fpath);

            string runTime = calcRunTime(timeStart, timeEnd);
            string str = "======================================"
                + "\r\nN\t" + LENGTH
                + "\r\nK\t" + DIMENSION
                + "\r\ntopK\t" + topK
                + "\r\nStart Time\t" + timeStart.ToString("yyyy-MM-dd HH:mm:ss")
                + "\r\nEnd Time\t" + timeEnd.ToString("yyyy-MM-dd HH:mm:ss")
                + "\r\nRun Time\t" + runTime;
            strfprintf(str, fpath);

            Console.WriteLine("ABOD Accomplished!");
        }

        #endregion


        #region Fast ABOD
        /// <summary>
        /// sort point @A 's all neighbor index by their distance to @A
        /// </summary>
        /// <param name="A"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        private static List<DPoint> sortIndexOfNborByDist(DStatusPacket A, int index)
        {
            List<DPoint> distList = new List<DPoint>(LENGTH - 1);
            DPoint tmp;
            double AB;  // dist(A, B)

            for (int i = 0; i < LENGTH; i++)
            {
                if (i == index)
                    continue;
                else if (i < index)
                    AB = distTable[i, index];
                else
                    AB = distTable[index, i];

                tmp = new DPoint(i, AB);  // i: the index of B in the database
                distList.Add(tmp);
            }
            distList.Sort();    // sort by tmp.Value (distance) ascending
            return distList;
        }


        /// <summary>
        /// get index of @kNN points of point @A in the database
        /// </summary>
        /// <param name="index"></param>
        /// <param name="kNN"></param>
        /// <returns>kNN Index</returns>
        private static int[] getkNNIndexOfA(DStatusPacket A, int index, int kNN)
        {
            int[] kNNIndex = new int[kNN];        // index of database, not ID

            List<DPoint> distList = sortIndexOfNborByDist(A, index);

            for (int i = 0; i < kNN; i++)
            {
                kNNIndex[i] = (int)distList[i].ID;
            }
            return kNNIndex;
        }


        /// <summary>
        /// calculate the approximate ABOF of point @A
        /// </summary>
        /// <param name="A"></param>
        /// <param name="index"></param>
        /// <param name="kNN"></param>
        /// <returns>ApproxABOF(A)</returns>
        private static double ApproxABOF(DStatusPacket A, int index, int kNN)
        {
            int[] kNNIndex = getkNNIndexOfA(A, index, kNN);
            List<DStatus> Nk = new List<DStatus>();
            DStatusPacket B;
            double variance;

            for (int i = 0; i < kNNIndex.Length; i++)
            {
                B = data.getDataByID(kNNIndex[i]);
                Nk.Add(addIndexToPacket(kNNIndex[i], B));
            }

            variance = ABOF(Nk, A, index);
            return variance;
        }



        /// <summary>
        /// Fast ABOD algorithm implement
        /// </summary>
        /// <param name="kNN"></param>
        /// <param name="topK"></param>
        /// <param name="fpath"></param>
        public static void FastABOD(int kNN, int topK, string fpath, DateTime timeStart)
        {
            DStatusPacket A;
            List<DPoint> FastABOFList = new List<DPoint>(LENGTH);
            DPoint tmp;
            double approxABOF_A;

            for (int i = 0; i < LENGTH; i++)
            {
                A = data.getDataByID(i);
                approxABOF_A = ApproxABOF(A, i, kNN);

                tmp = new DPoint(A.ID, approxABOF_A);
                FastABOFList.Add(tmp);
            }
            FastABOFList.Sort();

            DateTime timeEnd = DateTime.Now;

            saveDPoint(FastABOFList, fpath);
            saveOutlier(FastABOFList, topK, fpath);

            string runTime = calcRunTime(timeStart, timeEnd);
            string str = "======================================"
                + "\r\nN\t" + LENGTH
                + "\r\nK\t" + DIMENSION
                + "\r\nkNN\t" + kNN
                + "\r\ntopK\t" + topK
                + "\r\nStart Time: " + timeStart.ToString("yyyy-MM-dd HH:mm:ss")
                + "\r\nEnd Time: " + timeEnd.ToString("yyyy-MM-dd HH:mm:ss")
                + "\r\nRun Time: " + runTime;
            strfprintf(str, fpath);

            Console.WriteLine("Fast ABOD Accomplished!");
        }

        #endregion


        #region LB-ABOD

        /// <summary>
        /// calculate SUM (1 / (|AB| * |AC|) ^ power) for {B: 0-->D.Count(), C: 0-->D.Count()}
        /// </summary>
        /// <param name="D"></param>
        /// <param name="index"></param>
        /// <param name="power"></param>
        /// <returns>Sum_B(Sum_C((1/(|AB|*|AC|)^power)))</returns>
        private static double rcpcOfModePlot(List<DStatus> D, int index, int power)
        {
            double AB, AC;
            int indexB, indexC;
            double result = 0.0, tmp = 0.0;

            for (int i = 0; i < D.Count(); i++)     // for each B ∈ D
            {
                indexB = D[i].Index;
                if (indexB == index)
                    continue;
                if (indexB < index)
                    AB = distTable[indexB, index];
                else
                    AB = distTable[index, indexB];

                for (int j = i + 1; j < D.Count(); j++)     // for each C ∈ D
                {
                    indexC = D[j].Index;
                    if (indexC == index)
                        continue;
                    if (indexC < index)
                        AC = distTable[indexC, index];
                    else
                        AC = distTable[index, indexC];
                    try
                    {
                        tmp = 1 / (AB * AC);
                    }
                    catch (DivideByZeroException e)
                    {
                        Console.WriteLine("Overflow Exception:" + e.Message);
                    }
                    result += Math.Pow(tmp, power);
                }
            }
            return result;
        }


        /// <summary>
        /// calculate SUM (1 / (|AB| * |AC|) ^ power) for {B: 0-->D.Count(), C: 0-->D.Count()} in Linear Time
        /// </summary>
        /// <param name="D"></param>
        /// <param name="index"></param>
        /// <param name="power"></param>
        /// <returns></returns>
        private static double rcpcOfModePlot_Linear(List<DStatus> D, int index, int power)
        {
            double AB;
            int indexB;
            double result = 0.0, tmp = 0.0;

            for (int i = 0; i < D.Count(); i++)     // for each B ∈ D
            {
                indexB = D[i].Index;
                if (indexB == index)
                    continue;
                if (indexB < index)
                    AB = distTable[indexB, index];
                else
                    AB = distTable[index, indexB];
                try
                {
                    tmp = 1 / AB;
                }
                catch (DivideByZeroException e)
                {
                    Console.WriteLine("Overflow Exception:" + e.Message);
                }
                result += Math.Pow(tmp, power);
            }
            return result * result;
        }


        /// <summary>
        /// calculate @R2  of point @A in LB-ABOF
        /// </summary>
        /// <param name="D"></param>
        /// <param name="Nk"></param>
        /// <param name="index">index of point A</param>
        /// <returns>R2(A)</returns>
        private static double calcR2(List<DStatus> D, List<DStatus> Nk, int index)
        {
            double minuend = rcpcOfModePlot_Linear(D, index, 2);
            double subtranend = 2.0 * rcpcOfModePlot_Linear(Nk, index, 2);
            return (minuend - subtranend);
        }


        /// <summary>
        /// calculate LB-ABOF of point @A
        /// </summary>
        /// <param name="A"></param>
        /// <param name="index"></param>
        /// <param name="kNN"></param>
        /// <returns>LB-ABOF(A)</returns>
        private static double LB_ABOF(List<DStatus> D, DStatusPacket A, int index, int kNN)
        {
            int[] kNNIndex = getkNNIndexOfA(A, index, kNN);
            List<DStatus> Nk = new List<DStatus>(kNNIndex.Length);

            int indexB, indexC;
            DStatusPacket B, C;
            double AB, AC, dotProductOfABandAC, angle_BAC, R1 = 0.0, R2, tmp = 0.0;
            double Minuend_Numerator = 0.0, Subtrahend_Numerator = 0.0, Denominator;
            double Minuend = 0.0, Subtrahend = 0.0;

            for (int i = 0; i < kNNIndex.Length; i++)           // Nk
            {
                indexB = kNNIndex[i];
                if (indexB == index)
                    continue;
                B = data.getDataByID(indexB);
                Nk.Add(addIndexToPacket(indexB, B));          // k nearest neighbor of point A

                if (indexB < index)
                    AB = distTable[indexB, index];
                else
                    AB = distTable[index, indexB];
                for (int j = i + 1; j < kNNIndex.Length; j++)
                {
                    indexC = kNNIndex[j];
                    if (indexC == index || indexC == indexB)
                        continue;
                    C = data.getDataByID(indexC);
                    if (indexC < index)
                        AC = distTable[indexC, index];
                    else
                        AC = distTable[index, indexC];

                    //angle_BAC = calcAngleBAC(A, B, C, AB, AC);
                    dotProductOfABandAC = calcDotProduct(B - A, C - A);
                    try
                    {
                        tmp = dotProductOfABandAC / Math.Pow(AB * AC, 3);     // <AB, AC> / (|AB| * |AC|) ^ 3
                    }
                    catch (DivideByZeroException e)
                    {
                        Console.WriteLine("Overflow Exception:" + e.Message);
                    }
                    Minuend_Numerator += Math.Pow(tmp, 2) * AB * AC;
                    Subtrahend_Numerator += tmp;
                }
            }
            R2 = calcR2(D, Nk, index);
            //R1 = R2;

            Denominator = rcpcOfModePlot_Linear(D, index, 1);          // D
            try
            {
                Minuend = 2.0 * (Minuend_Numerator + R1) / Denominator;
                Subtrahend = Math.Pow(((2.0 * Subtrahend_Numerator + R2) / Denominator), 2);
            }
            catch (DivideByZeroException e)
            {
                Console.WriteLine("Deominator != 0" + e.Message);
            }
            return (Minuend - Subtrahend);      // return LB-ABOF(A)
        }


        /// <summary>
        /// code by H-P. Kerigel
        /// </summary>
        /// <param name="D"></param>
        /// <param name="A"></param>
        /// <param name="index"></param>
        /// <param name="kNN"></param>
        /// <returns></returns>
        private static double calcLBABOF(DStatusPacket A, int index, int kNN)
        {
            List<DPoint> NkIndex = new List<DPoint>();
            // Compute nearest neighbors and distances.

            double simAA = calcDotProduct(A, A);
            // Sum of 1./(|AB|) and 1./(|AB|^2); for computing R2.
            double sumid = 0, sumisqd = 0;
            for (int j = 0; j < LENGTH; j++)
            {
                if (index == j)
                {
                    continue;
                }
                DStatusPacket nB = data.getDataByID(j);
                double simBB = calcDotProduct(nB, nB);
                double simAB = calcDotProduct(A, nB);
                double sqdAB = simAA + simBB - simAB - simAB;

                if (!(sqdAB > 0))
                {
                    continue;
                }
                sumid += 1 / Math.Sqrt(sqdAB);
                sumisqd += 1 / sqdAB;
                // Update heap
                DPoint temp = new DPoint(j, sqdAB);
                if (NkIndex.Count < kNN)
                {
                    NkIndex.Add(temp);
                }
                else if (sqdAB < NkIndex.Max().Value)
                {
                    //移出最大的
                    NkIndex.Remove(NkIndex.Max());
                    NkIndex.Add(temp);
                }
            }

            // Compute FastABOD approximation, adjust for lower bound.
            // LB-ABOF is defined via a numerically unstable formula.
            // Variance as E(X^2)-E(X)^2 suffers from catastrophic cancellation!
            // TODO: ensure numerical precision!
            double nnsum = 0, nnsumsq = 0, nnsumisqd = 0;
            for (int k = 0; k < NkIndex.Count; k++)
            {
                DPoint iB = NkIndex[k];
                DStatusPacket nB = data.getDataByID(iB.ID);
                double sqdAB = iB.Value;
                double simAB = calcDotProduct(A, nB);
                if (!(sqdAB > 0))
                {
                    continue;
                }
                for (int l = 0; l < NkIndex.Count; l++)
                {
                    if (k == l)
                    {
                        continue;
                    }
                    DPoint iC = NkIndex[l];
                    DStatusPacket nC = data.getDataByID(iC.ID);
                    double sqdAC = iC.Value;
                    double simAC = calcDotProduct(A, nC);
                    if (!(sqdAC > 0))
                    {
                        continue;
                    }
                    // Exploit bilinearity of scalar product:
                    // <B-A, C-A> = <B, C-A> - <A,C-A>
                    // = <B,C> - <B,A> - <A,C> + <A,A>
                    double simBC = calcDotProduct(nB, nC);
                    double numerator = simBC - simAB - simAC + simAA;
                    double sqweight = 1 / (sqdAB * sqdAC);
                    double weight = Math.Sqrt(sqweight);
                    double val = numerator * sqweight;
                    nnsum += val * weight;
                    nnsumsq += val * val * weight;
                    nnsumisqd += sqweight;
                }
            }
            // Remaining weight, term R2:
            double r2 = sumisqd * sumisqd - 2 * nnsumisqd;
            double tmp = (2 * nnsum + r2) / (sumid * sumid);
            double lbabof = 2 * nnsumsq / (sumid * sumid) - tmp * tmp;

            return lbabof;
        }


        /// <summary>
        /// LB-ABOD algorithm implement
        /// </summary>
        /// <param name="kNN"></param>
        /// <param name="topK"></param>
        public static void LB_ABOD(int kNN, int topK, string fpath, DateTime timeStart)
        {
            DStatusPacket tmpPacket;
            List<DStatus> D = new List<DStatus>(LENGTH);

            for (int i = 0; i < LENGTH; i++)
            {
                tmpPacket = data.getDataByID(i);
                D.Add(addIndexToPacket(i, tmpPacket));
            }

            /*
             * (step 2) Compute LB-ABOF for each point  A ∈ D.
             * (step 3) Organize the database objects in a candidate list ordered ascending
             *          w.r.t. their assigned LB-ABOF.
             */
            DStatusPacket A;
            double LB_ABOF_A;
            List<DPoint> candidateList = new List<DPoint>();
            DPoint tmp;

            double ABOF_A;

            for (int i = 0; i < LENGTH; i++)
            {
                A = data.getDataByID(i);
                LB_ABOF_A = calcLBABOF(A, i, kNN);


                // debug
                //ABOF_A = ABOF(D, A, i);
                //if (ABOF_A - LB_ABOF_A <= 0)
                //{
                //    Console.WriteLine("ABOF(A) <= LB-ABOF(A)");
                //    Console.WriteLine("ABOF: {0}\tLB-ABOF: {1}", ABOF_A, LB_ABOF_A);
                //}

                tmp = new DPoint(i, LB_ABOF_A);
                candidateList.Add(tmp);

            }
            candidateList.Sort();           // sort ascending

            saveDPoint(candidateList, fpath);

            /*
             * (step 4) Determine the exact ABOF for the first @topK objects in the candidate
             *          list, Remove them from the candidate list and insert into the current
             *          result list.
             */
            int indexB;
            DStatusPacket B;
            double ABOF_B;

            int Counter = 0;        // The Counter of Comparable

            SortedSet<DPoint> resultList = new SortedSet<DPoint>();

            for (int i = 0; i < topK; i++)
            {
                indexB = (int)candidateList[i].ID;
                B = data.getDataByID(indexB);
                ABOF_B = ABOF(D, B, indexB);

                tmp = new DPoint(B.ID, ABOF_B);
                resultList.Add(tmp);

                candidateList.RemoveAt(i);
                Counter++;
            }
            /*
             * (step 6) if the largest ABOF in the result list < the smallest approximated 
             *          ABOF in the candidate list, terminate; else, proceed with step 5.
             * (step 5) Remove and examine the next best candidate C from the candidate list
             *          and determine the exact ABOF, if the ABOF of C < the largest ABOF
             *          of an object A in the result list, remove A from the result list and
             *          insert C into the result list.
             */
            int indexC;
            DStatusPacket C;    // next best candidate C in the candidate list
            DPoint CC;          // point that need to be insert into result list
            double ABOF_C;
            double Min_LBABOF = candidateList[0].Value;
            double Max_ABOF = resultList.Max().Value;

            while (Max_ABOF > Min_LBABOF && candidateList.Count() != 0)
            {
                indexC = (int)candidateList[0].ID;      // next best candidate
                C = data.getDataByID(indexC);
                ABOF_C = ABOF(D, C, indexC);
                candidateList.RemoveAt(0);

                if (ABOF_C < Max_ABOF)
                {
                    CC = new DPoint(C.ID, ABOF_C);
                    resultList.Remove(resultList.Max());
                    resultList.Add(CC);
                    
                    Counter++;

                    if (candidateList.Count() == 0)
                        break;
                    Min_LBABOF = candidateList[0].Value;
                    Max_ABOF = resultList.Max().Value;
                }
            }

            List<DPoint> reslist = new List<DPoint>();
            foreach (DPoint x in resultList)
            {
                reslist.Add(x);
                //Console.WriteLine("ID: " + x.ID + "\tMoteID: " + x.MoteID + "\tValue: " + x.Value);
            }
            //save result...

            DateTime timeEnd = DateTime.Now;

            saveDPoint(reslist, fpath);
            saveOutlier(reslist, topK, fpath);

            int numOutlier = 0;
            double precision;
            for (int k = 0; k < topK; k++)
            {
                if (reslist[k].ID % 50 == 0)
                    numOutlier++;
            }
            precision = numOutlier / topK;

            string runTime = calcRunTime(timeStart, timeEnd);
            string str = "======================================"
                + "\r\nN:\t" + LENGTH
                + "\r\nD:\t" + DIMENSION
                + "\r\nkNN:\t" + kNN
                + "\r\ntopK:\t" + topK
                + "\r\n======================================"
                + "\r\nCounter:\t" + Counter
                + "\r\nPrecision:\t" + precision
                + "\r\nStart Time:\t" + timeStart.ToString("yyyy-MM-dd HH:mm:ss")
                + "\r\nEnd Time:\t" + timeEnd.ToString("yyyy-MM-dd HH:mm:ss")
                + "\r\nRun Time:\t" + runTime;
            strfprintf(str, fpath);

            Console.WriteLine("LB-ABOD Accomplished!");
        }

        #endregion


        #region LOF


        /// <summary>
        /// get k nearest neighbor's index (DStatus with index of the data set)
        /// </summary>
        /// <param name="distList"></param>
        /// <param name="MinPts"></param>
        /// <returns></returns>
        private static List<int> getkNNIndex(List<DPoint> distList, int MinPts)
        {
            List<int> kNNIndex = new List<int>();
            int index;

            for (int i = 0; i < MinPts; i++)
            {
                index = distList[i].ID;
                kNNIndex.Add(index);
            }
            return kNNIndex;
        }


        /// <summary>
        /// get k-distance(P)
        /// </summary>
        /// <param name="distList"></param>
        /// <param name="MinPts"></param>
        /// <returns></returns>
        private static double getKDist(List<DkNNborIndex> kNNIndexOfAll, int index_P, int MinPts)
        {
            int index_O;
            DkNNborIndex kNNindex_P;
            double dist_PO;

            // get index of k nearest neighbor of point P
            kNNindex_P = kNNIndexOfAll[index_P];

            // The longest one's index
            index_O = kNNindex_P.kNNIndex[MinPts - 1];
            if (index_O == index_P)
                return 0;
            else if (index_O < index_P)
                dist_PO = distTable[index_O, index_P];
            else
                dist_PO = distTable[index_P, index_O];

            return dist_PO;
        }


        private static List<DStatus> getNk(List<int> kNNIndex)
        {
            List<DStatus> Nk_P = new List<DStatus>();

            int index_O;
            DStatusPacket NkElement;

            for (int i = 0; i < kNNIndex.Count(); i++)
            {
                index_O = kNNIndex[i];
                NkElement = data.getDataByID(index_O);

                Nk_P.Add(addIndexToPacket(index_O, NkElement));
            }
            return Nk_P;
        }


        /// <summary>
        /// calculate local reachability distance of P
        /// </summary>
        /// <param name="P"></param>
        /// <param name="index"></param>
        /// <param name="Nk"></param>
        /// <param name="MinPts"></param>
        /// <returns></returns>
        private static double LRD(List<DkNNborIndex> kNNIndexOfAll, int index_P, List<DStatus> Nk_P, int MinPts)
        {
            int index_O;

            double k_dist_O, dist_PO;
            double reach_dist_PO;

            double Denominator = 0.0;

            // for each O ∈ Nk(P)
            for (int i = 0; i < Nk_P.Count(); i++)
            {
                index_O = Nk_P[i].Index;
                if (index_O == index_P)
                    continue;

                // get k-distance(O)
                k_dist_O = getKDist(kNNIndexOfAll, index_O, MinPts);

                // get dist(P, O)
                if (index_O < index_P)
                    dist_PO = distTable[index_O, index_P];
                else
                    dist_PO = distTable[index_P, index_O];

                // get reach-dist(P, O)
                reach_dist_PO = Math.Max(k_dist_O, dist_PO);

                // Sigma( reach-dist(P, O) ) for O ∈ Nk(P)
                Denominator += reach_dist_PO;
            }
            return Nk_P.Count() / Denominator;
        }


        /// <summary>
        /// Local outlier factor of point P
        /// </summary>
        /// <param name="P"></param>
        /// <param name="index"></param>
        /// <param name="MinPts"></param>
        /// <returns></returns>
        private static double LOF(List<DkNNborIndex> kNNIndexOfAll, int index_P, int MinPts)
        {
            List<DStatus> Nk_P = getNk(kNNIndexOfAll[index_P].kNNIndex);
            double lrd_P = LRD(kNNIndexOfAll, index_P, Nk_P, MinPts);

            int index_O;
            List<DStatus> Nk_O;
            double lrd_O;

            double Numerator = 0.0;

            // for each point O ∈ Nk(P)
            for (int i = 0; i < Nk_P.Count(); i++)
            {
                index_O = Nk_P[i].Index;
                if (index_O == index_P)
                    continue;

                // get lrd(O)
                Nk_O = getNk(kNNIndexOfAll[index_O].kNNIndex);
                lrd_O = LRD(kNNIndexOfAll, index_O, Nk_O, MinPts);

                // Sigma( lrd(O) / lrd(P) ) for  O ∈ Nk(P)
                try
                {
                    Numerator += lrd_O / lrd_P;
                }
                catch (DivideByZeroException e)
                {
                    Console.WriteLine("Overflow Exception:" + e.Message);
                }
            }
            return Numerator / Nk_P.Count();
        }

        /// <summary>
        /// LOF: DBOD(Density Based Outlier Detection) algorithm implement
        /// </summary>
        /// <param name="MinPts"></param>
        /// <param name="fpath"></param>
        public static void DBOD(int MinPts, int topK, string fpath, DateTime timeStart)
        {
            List<DkNNborIndex> kNNIndexOfAll = new List<DkNNborIndex>(LENGTH);
            DkNNborIndex tmp;

            DStatusPacket P;
            List<DPoint> dist_list_P;
            List<int> kNNindex_P;

            for (int i = 0; i < LENGTH; i++)
            {
                P = data.getDataByID(i);
                // get sorted distance list of point P
                dist_list_P = sortIndexOfNborByDist(P, i);
                // get index of top k nearest neighbor
                kNNindex_P = getkNNIndex(dist_list_P, MinPts);

                tmp = new DkNNborIndex(i, kNNindex_P);
                kNNIndexOfAll.Add(tmp);
            }

            double LOF_P;

            List<DPoint> LOFList = new List<DPoint>(LENGTH);
            DPoint tmpPoint = new DPoint();

            for (int i = 0; i < LENGTH; i++)
            {
                P = data.getDataByID(i);
                LOF_P = LOF(kNNIndexOfAll, i, MinPts);

                //Console.WriteLine("{0}\t{1}", i, LOF_P);
                tmpPoint = new DPoint(P.ID, LOF_P);
                LOFList.Add(tmpPoint);
            }
            LOFList.Sort();

            DateTime timeEnd = DateTime.Now;

            // save result list...
            StreamWriter sw = File.AppendText(fpath);
            sw.WriteLine("======================================");
            sw.WriteLine("ID\tValue");
            foreach (DPoint x in LOFList)
                sw.WriteLine(x.ID + "\t" + x.Value);

            LOFList.Reverse();

            sw.WriteLine("==================outlier====================");
            for (int j = 0; j < topK; j++)
                sw.Write(LOFList[j].ID + "\t");


            string runTime = calcRunTime(timeStart, timeEnd);

            string str = "\r\n======================================"
                + "\r\nN\t" + LENGTH
                + "\r\nK\t" + DIMENSION
                + "\r\nMinPts\t" + MinPts
                + "\r\nStart Time\t" + timeStart.ToString("yyyy-MM-dd HH:mm:ss")
                + "\r\nEnd Time\t" + timeEnd.ToString("yyyy-MM-dd HH:mm:ss")
                + "\r\nRun Time\t" + runTime;
            sw.WriteLine(str);
            sw.Flush();
            sw.Close();

            Console.WriteLine("LOF Accomplished!");
        }

        #endregion


        #region Distance-based
        /// <summary>
        /// Distance based outlier detection algorithm implement
        /// </summary>
        /// <param name="MinDist"></param>
        /// <param name="pct"></param>
        /// <param name="fpath"></param>
        public static void DistanceBOD(double MinDist, float pct, string fpath)
        {
            DateTime timeStart = DateTime.Now;

            List<int> outlierList = new List<int>();
            int Count;
            bool isOutlier;
            double OP;

            for (int i = 0; i < LENGTH; i++)    // for each object P in hte data set
            {
                Count = 0;
                isOutlier = true;
                for (int j = 0; j < LENGTH; j++)    // for each neighbor of P in the data set
                {
                    if (i == j)
                        continue;
                    // get distance of O and P
                    if (i < j)
                        OP = distTable[i, j];
                    else
                        OP = distTable[j, i];

                    if (OP <= MinDist)
                        Count++;
                    if (Count >= LENGTH * pct)      // P is not a DB(MinDist, pct) outlier
                    {
                        isOutlier = false;
                        break;
                    }
                }
                if (isOutlier)
                {
                    outlierList.Add(i);
                }
            }
            DateTime timeEnd = DateTime.Now;

            // save result list...
            StreamWriter sw = File.AppendText(fpath);
            sw.WriteLine("==================outlier====================");
            foreach (int x in outlierList)
                sw.Write(x + "\t");

            string runTime = calcRunTime(timeStart, timeEnd);

            string str = "======================================"
                + "\r\nN\t" + LENGTH
                + "\r\nK\t" + DIMENSION
                + "\r\nMin Dist\t" + MinDist
                + "\r\npercentage\t" + pct
                + "\r\nStart Time\t" + timeStart.ToString("yyyy-MM-dd HH:mm:ss")
                + "\r\nEnd Time\t" + timeEnd.ToString("yyyy-MM-dd HH:mm:ss")
                + "\r\nRun Time\t" + runTime;
            sw.WriteLine(str);

            sw.Flush();
            sw.Close();
            Console.WriteLine("Distance algorithm Accomplished!");
        }

        #endregion



        public static void run(int kNN, int topK, string folder)
        {
            DateTime startTime = DateTime.Now;
            calcDistMatrix();

            string fABOD = folder + "\\ABOD_N" + LENGTH + "_D" + DIMENSION + "_topK" + topK + ".txt";
            //string fFastABOD = folder + "\\FastABOD_N" + LENGTH + "_D" + DIMENSION + "_kNN" + kNN + "_topK" + topK + ".txt";
            //string fLOF = folder + "\\LOF_N" + LENGTH + "_D" + DIMENSION + "_kNN" + kNN + "_topK" + topK + ".txt";

            //DBOD(kNN, topK, fLOF, startTime);
            //FastABOD(kNN, topK, fFastABOD, startTime);
            ABOD(topK, fABOD, startTime);
        }


        public static void runLBABOD(int kNN, int topK, string folder)
        {
            DateTime startTime = DateTime.Now;
            calcDistMatrix();

            string fLBABOD = folder + "\\LBABOD_N" + LENGTH + "_D" + DIMENSION + "_kNN" + kNN + "_topK" + topK + ".txt";

            LB_ABOD(kNN, topK, fLBABOD, startTime);

        }


        /// <summary>
        /// Main
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            /*
             * To run the program, you need to change the following parameters:
             * m_IdString:      The ID of the database server.          SQLServerDBUtil.cs
             * m_PassString:    The Password of the database server.    SQLServerDBUtil.cs
             * m_DbString:      The database name.                      SQLServerDBUtil.cs
             * LENGTH:          Total number of points to be calculated Program.cs
             * DIMENSION:       The dimensionality of data table.       Program.cs
             * topK:            Top K points are outlier.               Program.cs
             * kNN:             k nearest neighbor.                     Program.cs
             */

            int topK, kNN;

            DateTime dt = DateTime.Now;
            string strCurrTime = dt.ToString("yyyyMMddHHmmss");
            //string folder = @"D:\Database\N" + LENGTH + "_D" + DIMENSION + "_" + strCurrTime;


            string folder = @"D:\Database\N500_维度测试_LBABOD_20140612165637";
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            
            kNN = 300;
            topK = LENGTH / 50;

            runLBABOD(kNN, topK, folder);


        }
    }
}
