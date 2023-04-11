using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Lab17
{
    public partial class Form1 : Form
    {
        int i = 0, Num;
        double lyam1, lyam2;
        double[] freq,  tau; double t0 = 0, t1 = 0.5;
        double[] potok1, potok2, aggr_potok;
        double chi;

        Random r = new Random();
        int n;

        public Form1()
        {
            InitializeComponent();
            Num = 100000;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            lyam1 = (double)numericUpDown1.Value;
            lyam2 = (double)numericUpDown2.Value;
            potok1 = new double[Num];
            potok2 = new double[Num];
            potok1[0] = t0;
            potok2[0] = t1;

            // сгенерировать поток 1 и поток 2
            for (i = 1; i< Num; i++)
            {
                potok1[i] = potok1[i - 1] + Math.Log(r.NextDouble()) / lyam1;
                potok2[i] = potok2[i - 1] + Math.Log(r.NextDouble()) / lyam2;
            }

            // слить их в один поток
            aggr_potok = new double[2*Num];
            int M = 2 + Num;

            potok1.CopyTo(aggr_potok, 0);
            potok2.CopyTo(aggr_potok, Num);

            // отсортировать по возрастанию
            MergeSort(aggr_potok, 0, M-1);

            // создать выборку tau разниц t[i] - t[i-1]
            tau = new double[M - 1];
            for( int i = 1, j = 0; i<M-1; i++, j++)
            {
                tau[j] = aggr_potok[i] - aggr_potok[i - 1];
            }

            // проверить хи квадратом распределение непрерывной случайной величины tau (сравниваем с экспоненциальным распределением с параметром (л1 + л2)

            n = (int)Math.Log((double)M) + 1; // кол-во интервалов 

            double min = tau.Min();
            double h = (tau.Max() - tau.Min()) / (double)n; // дельта
            freq = new double[n]; for (int i = 0; i < n; i++) freq[i] = 0;

            for (int i = 0; i < M-1; i++)
            {
                for (int j = 1; j <= n; j++)
                {
                    if (tau[i] < min + j * h) { freq[j - 1]++; break; }
                }
            }
            for (int i = 0; i < n; i++) freq[i] = freq[i] / (double)M; // частота 

            chart1.Series[0].Points.Clear();
            for (int i = 0; i < n; i++) chart1.Series[0].Points.AddXY(min + (i+1) * h/(double)2, freq[i]);

            textBox2.Text = isChiSquared(n, M, freq, h, min).ToString();
        }

        double prob(double l, double a, double b)
        {
            return Math.Exp(-l * b) - Math.Exp(-l * a);
        }


        bool isChiSquared(int m, int N, double[] freq, double h, double min)
        {
            double square_hi = 19.675;
            chi = 0;
            double a = min;
            double b = min + h;
            for(int i = 0; i<m; i++)
            {
                chi += freq[i] * freq[i] / ((double)N* prob(lyam1 + lyam2, a, b));
                a = b; b += h;
            }
            chi -= (double)N;

            return (chi <= square_hi);
        }

        static void Merge(double[] array, int lowIndex, int middleIndex, int highIndex)
        {
            var left = lowIndex;
            var right = middleIndex + 1;
            var tempArray = new double[highIndex - lowIndex + 1];
            var index = 0;

            while ((left <= middleIndex) && (right <= highIndex))
            {
                if (array[left] < array[right])
                {
                    tempArray[index] = array[left];
                    left++;
                }
                else
                {
                    tempArray[index] = array[right];
                    right++;
                }

                index++;
            }

            for (var i = left; i <= middleIndex; i++)
            {
                tempArray[index] = array[i];
                index++;
            }

            for (var i = right; i <= highIndex; i++)
            {
                tempArray[index] = array[i];
                index++;
            }

            for (var i = 0; i < tempArray.Length; i++)
            {
                array[lowIndex + i] = tempArray[i];
            }
        }

        //сортировка слиянием
        static double[] MergeSort(double[] array, int lowIndex, int highIndex)
        {
            if (lowIndex < highIndex)
            {
                var middleIndex = (lowIndex + highIndex) / 2;
                MergeSort(array, lowIndex, middleIndex);
                MergeSort(array, middleIndex + 1, highIndex);
                Merge(array, lowIndex, middleIndex, highIndex);
            }

            return array;
        }
    }
}
