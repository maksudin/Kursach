using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AleksandrovKurs
{
    public partial class Form1 : Form
    {

        private const double max_time = 3600;    // поменять на 3600 

        public class CmoModel
        {
            public double n { get; set; }           // Количество серверов
            public double l { get; set; }           // (Лямбда) интенсивность поступления программ в секунду
            public double M { get; set; }           // (Лямбда) интенсивность интенсивность потока обслуживания
            public double Tobr { get; set; }        // время обработки проги 

            public double p0 { get; set; }          // Вероятность, что ВС не загружена
            public double p1 { get; set; }          // Вероятность, что загружен один сервер
            public double p2 { get; set; }          // Вероятность, что загружен два серва
            public double p3 { get; set; }          // Вероятность, что загружен три серва
            public double p4 { get; internal set; }
            public double p5 { get; internal set; }
            public double p6 { get; internal set; }

            public double q { get; set; }           // Относительная пропускная способность
            public double s { get; set; }           // Абсолютная пропускная способность
            public double p_denied { get; set; }    // Вероятность отказа
            public double k { get; set; }           // Среднее число занятых серверов

            // Линейное распределение поступления программ
            public double Tzmin { get; set; }
            public double Tzmax { get; set; }
            // Линейное распределение времени обработки программ
            public double Tsmin { get; set; }
            public double Tsmax { get; set; }
        }


        public Form1()
        {
            InitializeComponent();
           
        }

        // Линейное распределение
        private void button1_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
            double[] servers = { 0, 0, 0 };
            Queue<Double> buffer = new Queue<Double>();
            double time = 0, task_interval = 0, task_finish_time = 0;
            int tasks_denied = 0, tasks_finished = 0, tasks = 0;
            double time_finish_sum = 0;
            bool servers_full;

            CmoModel cmo = new CmoModel();
            Random r = new Random();
            setupVars(cmo);

            while (time <= max_time)
            {
                servers_full = true;
                task_interval = r.NextDouble() * (cmo.Tzmax - cmo.Tzmin) + cmo.Tzmin;             // промежуток прихода задачи
                //Console.WriteLine("task_interval=" + task_interval);
                time += task_interval;
                tasks++;                                                                          // всего задач

                for (int i = 0; i < 3; i++)
                {
                    servers[i] -= task_interval;
                }


                for(int i = 0; i < 3; i++)
                {
                    if (servers[i] <= 0)
                    {
                        if (buffer.Count == 0)
                        {
                            task_finish_time = r.NextDouble() * (cmo.Tsmax - cmo.Tsmin) + cmo.Tsmin;   // время обработки задачи
                        }
                        else
                        {
                            task_finish_time = buffer.Dequeue();
                            //Console.WriteLine("buffer:");
                        }

                        servers[i] = task_finish_time;
                        tasks_finished++;                                                              // обработанные задачи
                        servers_full = false;
                        break;
                    }
                }

                if (servers_full == true)
                {
                    if (buffer.Count < 3)
                    {
                        task_finish_time = r.NextDouble() * (cmo.Tsmax - cmo.Tsmin) + cmo.Tsmin;   // время обработки задачи
                        buffer.Enqueue(task_finish_time);
                    }
                    tasks_denied++;                                                                 // необработанные задачи
                }
                time_finish_sum += task_finish_time;
            }

            cmo.l = tasks / max_time;
            cmo.Tobr = time_finish_sum / tasks;                                                     // среднее время обработки
            cmo.M = 1 / cmo.Tobr;                                                                   // интенсивность потока обслуживания
            double P = cmo.l / cmo.M;                                                               // Интенсивность загрузки = лямбда(2) / M
            Console.WriteLine(P);
            int n = 3;  // количество серверовMath.Pow(P, n + 1) / n * 6 * cmo.;
            int m = 3;  // размер буфера
            cmo.p0 = Math.Pow( ( 1 + P + (Math.Pow(P, 2) / 2) + (Math.Pow(P, 3) / 6) + 
                (Math.Pow(P, n + 1) * Math.Pow(1 - (P / n), m) / (n * 6 * (1 - (P / n) ) ))), -1);
            cmo.p1 = P / 1 * cmo.p0;
            cmo.p2 = Math.Pow(P, 2) / 2 * cmo.p0;
            cmo.p3 = Math.Pow(P, 3) / 6 * cmo.p0;
            cmo.p4 = 0;       // n! = 6
            cmo.p5 = 0;        // n! = 6
            cmo.p6 = 0;        // n! = 6
            cmo.p_denied = cmo.p3;
            cmo.q = 1 - cmo.p_denied;
            cmo.s = cmo.l * cmo.q;
            cmo.k = cmo.s / cmo.M;


            textBox1.Clear();
            textBox1.AppendText("" + tasks);
            textBox3.Clear();
            textBox3.AppendText("" + tasks_finished);
            textBox2.Clear();
            textBox2.AppendText("" + tasks_denied);


            richTextBox1.AppendText(String.Format("P0={0:0.0000}\n", cmo.p0));
            richTextBox1.AppendText(String.Format("P1={0:0.0000}\n", cmo.p1));
            richTextBox1.AppendText(String.Format("P2={0:0.0000}\n", cmo.p2));
            richTextBox1.AppendText(String.Format("P3={0:0.0000}\n", cmo.p3));
            richTextBox1.AppendText(String.Format("P4={0:0.0000}\n", cmo.p4));
            richTextBox1.AppendText(String.Format("P5={0:0.0000}\n", cmo.p5));
            richTextBox1.AppendText(String.Format("P6={0:0.0000}\n", cmo.p6));
            richTextBox1.AppendText(String.Format("Q={0:0.0000}\n", cmo.q));
            richTextBox1.AppendText(String.Format("S={0:0.0000}" + " задач/сек\n", cmo.s));
            richTextBox1.AppendText(String.Format("Pотк={0:0.0000}\n", cmo.p_denied));
            richTextBox1.AppendText(String.Format("K={0:0.0000}\n ", cmo.k));
            richTextBox1.AppendText(String.Format("Nпрог={0:0.0000}\n ", cmo.k));
            richTextBox1.AppendText(String.Format("Tпрог={0:0.0000}\n ", cmo.k));
            richTextBox1.AppendText(String.Format("Nбуф={0:0.0000}\n ", cmo.k));
            richTextBox1.AppendText(String.Format("Tбуф={0:0.0000}\n ", cmo.k));
            
            }
        
        // Экспоненциальное распределение

        private void button2_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
            double[] servers = { 0, 0, 0 };
            Queue<Double> buffer = new Queue<Double>();
            double time = 0, task_interval = 0, task_finish_time = 0, u = 0;
            int tasks_denied = 0, tasks_finished = 0, tasks = 0;
            double time_finish_sum = 0;
            bool servers_full;

            CmoModel cmo = new CmoModel();
            Random r = new Random();
            setupVars(cmo);

            while (time <= max_time)
            {
                servers_full = true;
                u = r.NextDouble();
                task_interval = Math.Log10(1 - u) / (-2);          // промежуток прихода задачи
                Console.WriteLine(task_interval);
                time += task_interval;
                tasks++;                                                                          // всего задач

                Console.WriteLine("servers:");
                for (int i = 0; i < 3; i++)
                {
                    servers[i] -= task_interval;
                    Console.Write(" " + servers[i]);
                }
                Console.WriteLine();


                for(int i = 0; i < 3; i++)
                {
                    if (servers[i] <= 0)
                    {
                        Console.WriteLine("i=" + i);
                        Console.WriteLine("Count=" + buffer.Count);
                        if (buffer.Count == 0)
                        {
                            u = r.NextDouble();
                            task_finish_time = Math.Log10(1 - u) / (-2) * 3;          // промежуток прихода задачи
                        }
                        else
                        {
                            task_finish_time = buffer.Dequeue();
                            //Console.WriteLine("buffer:");

                            Console.WriteLine("buffer:");
                            foreach (Double buf in buffer)
                            {
                                Console.Write(" " + buf);
                            }
                            Console.WriteLine();
                        }

                        Console.WriteLine("task_finish_time=" + task_finish_time);
                        servers[i] = task_finish_time;
                        tasks_finished++;                                                              // обработанные задачи
                        servers_full = false;
                        break;
                    }
                }

                if (servers_full == true)
                {
                    if (buffer.Count < 3)
                    {
                        u = r.NextDouble();
                        task_finish_time = Math.Log10(1 - u) / (-2) * 3;          // промежуток прихода задачи
                        buffer.Enqueue(task_finish_time);
                        Console.WriteLine("buffer:");
                        foreach (Double buf in buffer)
                        {
                            Console.Write(" " + buf);
                        }
                        Console.WriteLine();
                    }
                    tasks_denied++;                                                                 // необработанные задачи
                }
                time_finish_sum += task_finish_time;
            }

            cmo.l = tasks / max_time;
            cmo.Tobr = time_finish_sum / tasks;                                                     // среднее время обработки
            cmo.M = 1 / cmo.Tobr;                                                                   // интенсивность потока обслуживания
            double P = cmo.l / cmo.M;                                                               // Интенсивность загрузки = лямбда(2) / M
            cmo.p0 = Math.Pow( ( 1 + P + (Math.Pow(P, 2) / 2) + (Math.Pow(P, 3) / 6) ), -1);
            cmo.p1 = P * cmo.p0;
            cmo.p2 = Math.Pow(P, 2) / 2 * cmo.p0;
            cmo.p3 = Math.Pow(P, 3) / 6 * cmo.p0;
            cmo.p_denied = cmo.p3;
            cmo.q = 1 - cmo.p_denied;
            cmo.s = cmo.l * cmo.q;
            cmo.k = cmo.s / cmo.M;


            textBox1.Clear();
            textBox1.AppendText("" + tasks);
            textBox3.Clear();
            textBox3.AppendText("" + tasks_finished);
            textBox2.Clear();
            textBox2.AppendText("" + tasks_denied);


            richTextBox1.AppendText(String.Format("P0={0:0.0000}\n", cmo.p0));
            richTextBox1.AppendText(String.Format("P1={0:0.0000}\n", cmo.p1));
            richTextBox1.AppendText(String.Format("P2={0:0.0000}\n", cmo.p2));
            richTextBox1.AppendText(String.Format("P3={0:0.0000}\n", cmo.p3));
            richTextBox1.AppendText(String.Format("Q={0:0.0000}\n", cmo.q));
            richTextBox1.AppendText(String.Format("S={0:0.0000}" + " задач/сек\n", cmo.s));
            richTextBox1.AppendText(String.Format("Pотк={0:0.0000}\n", cmo.p_denied));
            richTextBox1.AppendText(String.Format("K={0:0.0000}\n ", cmo.k));
            richTextBox1.AppendText(String.Format("Nпрог={0:0.0000}\n ", cmo.k));
            richTextBox1.AppendText(String.Format("Tпрог={0:0.0000}\n ", cmo.k));
            richTextBox1.AppendText(String.Format("Nбуф={0:0.0000}\n ", cmo.k));
            richTextBox1.AppendText(String.Format("Tбуф={0:0.0000}\n ", cmo.k));
            
        }


        private void setupVars(CmoModel cmo)
        {
            cmo.Tzmax = 0.83;
            cmo.Tzmin = 0.5;
            cmo.Tsmax = 1.00;
            cmo.Tsmin = 5.00;
 
        }

        // Unneccessary methods
        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void label14_Click(object sender, EventArgs e)
        {

        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }

    }
}
