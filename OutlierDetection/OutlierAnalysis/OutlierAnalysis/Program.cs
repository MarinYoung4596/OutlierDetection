using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using OutlierAnalysis.SysUtil;

namespace OutlierAnalysis
{
    class Program
    {
        private static List<int> readFile(string fpath, int topK, double r, bool flag)
        {
            List<int> numOfMoteID = new List<int>();
            FileStream fs = new FileStream(fpath, FileMode.Open);
            StreamReader sr = new StreamReader(fs, Encoding.GetEncoding("GBK"));

            string strLine;
            string[] strArray;
            int tmp;
            sr.BaseStream.Seek(0, SeekOrigin.Begin);

            // LOF
            if (flag == true)
            {
                while (!sr.EndOfStream)
                {
                    strLine = sr.ReadLine();
                    strArray = strLine.Split('\t');
                    if (Double.Parse(strArray[2]) < r)
                        break;

                    tmp = Int32.Parse(strArray[1]);
                    numOfMoteID.Add(tmp);
                }
            }
            else // flag == false; Fast ABOD
            {
                int i = 0;
                while (!sr.EndOfStream && i < topK)
                {
                    strLine = sr.ReadLine();
                    strArray = strLine.Split('\t');
                    tmp = Int32.Parse(strArray[1]);
                    numOfMoteID.Add(tmp);
                    i++;
                }
            }
            //foreach (int x in numOfMoteID)
            //    Console.WriteLine(x);
            return numOfMoteID;
        }


        private static List<int> read_file(string fpath)
        {
            List<int> numOfMoteID = new List<int>();
            FileStream fs = new FileStream(fpath, FileMode.Open);
            StreamReader sr = new StreamReader(fs, Encoding.GetEncoding("GBK"));

            string strLine;
            string[] strArray;
            int tmp;
            sr.BaseStream.Seek(0, SeekOrigin.Begin);

            while (!sr.EndOfStream)
            {
                strLine = sr.ReadLine();
                strArray = strLine.Split('\t');

                tmp = Int32.Parse(strArray[1]);
                numOfMoteID.Add(tmp);
            }
            return numOfMoteID;
        }

        private static int getIndexOfMoteID(List<DPoint> list, int MoteID)
        {
            for (int i = 0; i < list.Count(); i++)
            {
                if (list[i].MoteID == MoteID)
                    return i;
            }
            return -1;
        }


        public static void run(string fLOF, double r, string fFastABOD, int topK, string fResult)
        {
            List<int> outlierLOF = readFile(fLOF, topK, r, true);
            List<int> outlierFastABOD = readFile(fFastABOD, topK, r, false);

            List<DPoint> resultList = new List<DPoint>();
            DPoint tmp;
            int index;

            for (int i = 0; i < outlierLOF.Count; i++)
            {
                index = getIndexOfMoteID(resultList, outlierLOF[i]);
                if (resultList.Count() == 0 || index == -1)
                {
                    tmp = new DPoint();
                    tmp.MoteID = outlierLOF[i];
                    tmp.NumLOF = 1;

                    resultList.Add(tmp);
                }
                else
                    resultList[index].NumLOF++;
            }
            for (int j = 0; j < outlierFastABOD.Count(); j++)
            {
                index = getIndexOfMoteID(resultList, outlierFastABOD[j]);
                if (resultList.Count() == 0 || index == -1)
                {
                    tmp = new DPoint();
                    tmp.MoteID = outlierFastABOD[j];
                    tmp.NumFastABOD = 1;

                    resultList.Add(tmp);
                }
                else
                    resultList[index].NumFastABOD++;
            }


            string fNoise = @"D:/noise.txt";
            List<int> noise = read_file(fNoise);
            for (int k = 0; k < noise.Count(); k++)
            {
                index = getIndexOfMoteID(resultList, noise[k]);
                if (index == -1)
                {
                    tmp = new DPoint();
                    tmp.MoteID = outlierFastABOD[k];
                    tmp.NumNoise = 1;

                    resultList.Add(tmp);
                }
                else
                    resultList[index].NumNoise++;
            }

            foreach (DPoint x in resultList)
                x.sumNum();

            resultList.Sort();
            resultList.Reverse();

            StreamWriter sw = File.AppendText(fResult);
            string str;
            int nLOF = 0, nFastABOD = 0, nTotal = 0, nDiff = 0;
            sw.WriteLine("======================================");
            sw.WriteLine("MoteID\tLOF\tFastABOD\tNoise\tSum");
            foreach (DPoint x in resultList)
            {
                if ((x.NumLOF == 0 && x.NumFastABOD == 1) || x.NumLOF == 1 && x.NumFastABOD == 0)
                    nDiff++;
                nFastABOD += x.NumFastABOD;
                nLOF += x.NumLOF;
                nTotal += x.Sum;

                str = String.Format("{0}\t{1}\t{2}\t{3}\t{4}", x.MoteID, x.NumLOF, x.NumFastABOD, x.NumNoise, x.Sum);
                sw.WriteLine(str);
            }

            str = "======================================"
                + "\r\nNum LOF\t" + nLOF
                + "\r\nNum Fast ABOD\t" + nFastABOD
                + "\r\nNum Different\t" + nDiff
                + "\r\nNum Total\t" + resultList.Count()
                + "\r\nDifference\t" + nDiff / nTotal;
            sw.WriteLine(str);

            sw.Flush();
            sw.Close();
        }


        static void Main(string[] args)
        {
            string strCurrTime = DateTime.Now.ToString("yyyyMMddHHmmss");
            string fLOF = @"D:/LOF.txt";
            string fFastABOD = @"D:/FastABOD.txt";
            string fResult = @"D:/Analyze" + strCurrTime + ".txt";

            int topK = 500;
            double r = 2.24363740543061;

            run(fLOF, r, fFastABOD, topK, fResult);

        }
    }
}
