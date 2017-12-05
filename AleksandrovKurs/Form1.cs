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

        private const double max_time = 3600;

        public class CmoModel
        {
            public double n { get; set; }           // Количество серверов
            public double l { get; set; }           // (Лямбда) интенсивность поступления программ в секунду
            public double m { get; set; }           // (Лямбда) интенсивность интенсивность потока обслуживания
            public double Tobr { get; set; }        // время обработки проги 

            public double p0 { get; set; }          // Вероятность, что ВС не загружена
            public double p1 { get; set; }          // Вероятность, что загружен один сервер
            public double p2 { get; set; }          // Вероятность, что загружен два серва
            public double p3 { get; set; }          // Вероятность, что загружен три серва

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

        private void button1_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
            double[] servers = {0, 0, 0};
            double time = 0, task_interval = 0, task_finish_time = 0;
            int tasks_denied = 0, tasks_finished = 0, tasks = 0;
            double time_finish_sum = 0;
            bool servers_full;

            CmoModel cmo = new CmoModel();
            setupVars(cmo);

            Random r = new Random();

            while(time <= max_time) // поменять на 3600
            {
                servers_full = true;
                task_interval = GetRandomNumber(cmo.Tzmin, cmo.Tzmax);             // промежуток прихода задачи
                time += task_interval;
                tasks++;                                           // всего задач


                for(int i = 0; i < 3; i++)
                {
                    servers[i] -= task_interval;
                }

                for(int i = 0; i < 3; i++)
                {
                    if (servers[i] <= 0)
                    {
                        task_finish_time = GetRandomNumber(cmo.Tsmin, cmo.Tsmax);   // время обработки задачи
                        servers[i] = task_finish_time;
                        tasks_finished++;                               // обработанные задачи
                        servers_full = false;
                        time_finish_sum += task_finish_time;
                        break;
                    }
                }

                if (servers_full == true)
                {
                    tasks_denied++;                                     // необработанные задачи
                }
            }

            cmo.l = tasks / max_time;
            cmo.Tobr = time_finish_sum / tasks;                         // среднее время обработки
            cmo.m = 1 / cmo.Tobr;                                       // интенсивность потока обслуживания
            double P = cmo.l / cmo.m;                                   // Интенсивность загрузки = лямбда(2) / M
            cmo.p0 = Math.Pow( ( 1 + P + (Math.Pow(P, 2) / 2) + (Math.Pow(P, 3) / 6) ), -1);
            cmo.p1 = P * cmo.p0;
            cmo.p2 = Math.Pow(P, 2) / 2 * cmo.p0;
            cmo.p3 = Math.Pow(P, 3) / 6 * cmo.p0;
            cmo.p_denied = cmo.p3;
            cmo.q = 1 - cmo.p_denied;
            cmo.s = cmo.l * cmo.q;
            cmo.k = cmo.s / cmo.m;


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
        }


        private void button2_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
            double[] servers = {0, 0, 0};
            double time = 0, task_interval = 0, task_finish_time = 0;
            int tasks_denied = 0, tasks_finished = 0, tasks = 0;
            double time_finish_sum = 0, u = 0, v = 0;
            bool servers_full;

            CmoModel cmoe = new CmoModel();

            Random r = new Random();

            while(time <= max_time) // поменять на 3600
            {
                servers_full = true;
                u = r.NextDouble();
                task_interval = Math.Log10(1 - u) / (-2);          // промежуток прихода задачи
                time += task_interval;
                tasks++;                                           // всего задач

                for(int i = 0; i < 3; i++)
                {
                    servers[i] -= task_interval;
                }

                for(int i = 0; i < 3; i++)
                {
                    if (servers[i] <= 0)
                    {
                        v = r.NextDouble();                                         // время обработки задачи
                        task_finish_time = Math.Log10(1 - v) / (-3) * 3;            // домнажаем ещё на 3 чтобы распределение было от 0 до 4 
                        Console.WriteLine(task_finish_time);                        // и да экспонента бывает и больше единицы иногда
                        servers[i] = task_finish_time;
                        tasks_finished++;                                           // обработанные задачи
                        servers_full = false;
                        time_finish_sum += task_finish_time;
                        break;
                    }
                }

                if (servers_full == true)
                {
                    tasks_denied++;                                     // необработанные задачи
                }
            }

            cmoe.l = tasks / max_time;
            cmoe.Tobr = time_finish_sum / tasks;                         // среднее время обработки
            cmoe.m = 1 / cmoe.Tobr;                                       // интенсивность потока обслуживания
            double P = cmoe.l / cmoe.m;                                   // Интенсивность загрузки = лямбда(2) / M
            cmoe.p0 = Math.Pow( ( 1 + P + (Math.Pow(P, 2) / 2) + (Math.Pow(P, 3) / 6) ), -1);
            cmoe.p1 = P * cmoe.p0;
            cmoe.p2 = Math.Pow(P, 2) / 2 * cmoe.p0;
            cmoe.p3 = Math.Pow(P, 3) / 6 * cmoe.p0;
            cmoe.p_denied = cmoe.p3;
            cmoe.q = 1 - cmoe.p_denied;
            cmoe.s = cmoe.l * cmoe.q;
            cmoe.k = cmoe.s / cmoe.m;

            textBox1.Clear();
            textBox1.AppendText("" + tasks);
            textBox3.Clear();
            textBox3.AppendText("" + tasks_finished);
            textBox2.Clear();
            textBox2.AppendText("" + tasks_denied);


            richTextBox1.AppendText(String.Format("P0={0:0.0000}\n", cmoe.p0));
            richTextBox1.AppendText(String.Format("P1={0:0.0000}\n", cmoe.p1));
            richTextBox1.AppendText(String.Format("P2={0:0.0000}\n", cmoe.p2));
            richTextBox1.AppendText(String.Format("P3={0:0.0000}\n", cmoe.p3));
            richTextBox1.AppendText(String.Format("Q={0:0.0000}\n", cmoe.q));
            richTextBox1.AppendText(String.Format("S={0:0.0000}" + " задач/сек\n", cmoe.s));
            richTextBox1.AppendText(String.Format("Pотк={0:0.0000}\n", cmoe.p_denied));
            richTextBox1.AppendText(String.Format("K={0:0.0000}\n ", cmoe.k));

        }


        private void setupVars(CmoModel cmo)
        {
            cmo.Tzmax = 0.333;
            cmo.Tzmin = 0.666;
            cmo.Tsmax = 6.0;
            cmo.Tsmin = 1.0;
 
        }

        // Helper methods
        public double GetRandomNumber(double minimum, double maximum)
        {
            Random random = new Random();
            return random.NextDouble() * (maximum - minimum) + minimum;
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
