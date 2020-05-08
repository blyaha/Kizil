using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Management;
using System.Threading;

namespace Load_CPU_RAM
{
    public partial class MainForm : Form
    {
       
        int numberOfStressedCore;
        Thread[] threads;
        int powerFactorOfTest;

        // multiplier - коэффициент скорости заполнения
        public static int multiplier;

        public MainForm()
        {
            InitializeComponent();

            btnPauseFillRAM.Enabled = false; 
            btnPauseFillRAM.BackColor = Color.LightGray;

            btnFreeRAM.Enabled = false;
            btnFreeRAM.BackColor = Color.LightGray;

            btnStop.Enabled = false;
            btnStop.BackColor = Color.LightGray;

            lblTotalCoreValue.Text = Environment.ProcessorCount.ToString();
            nudReservedCoreValue.Maximum = Environment.ProcessorCount - 1;
        }

        bool lblUsedFreeRAM_usage = true; 
        private void lblUsedFreeRAM_Click(object sender, EventArgs e)
        {
            if (lblUsedFreeRAM_usage)
            {
                lblUsedFreeRAM_usage = false;
            }
            else
            {
                lblUsedFreeRAM_usage = true;
            }
        }

        bool lblUsedFreeRAMValue_percent = true; 
        private void lblUsedFreeRAMValue_Click(object sender, EventArgs e)
        {
            if (lblUsedFreeRAMValue_percent)
            {
                lblUsedFreeRAMValue_percent = false;
            }
            else
            {
                lblUsedFreeRAMValue_percent = true;
            }
        }


        // Запрос к WMI для получения памяти ПК
        ManagementObjectSearcher ramMonitor = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_OperatingSystem");

        // Запрос к WMI для получения загрузки ЦП
        ManagementObjectSearcher cpuMonitor = new ManagementObjectSearcher("select * from Win32_PerfFormattedData_PerfOS_Processor");


        double totalRam, freeRam, busyRam; // Объявляем переменные для хранения объёмов памяти (общего, свободного и занятого)

        bool fillingRAM = false; // true - запущен процесс заполнения памяти; false - процесс заполнения остановлен

        // Вызов события сработал таймер
        private void tmrCheckRAMSizeAndCPULoad_Tick(object sender, EventArgs e)
        {
            // Забираем в цикле искомые значения объёмов памяти
            foreach (ManagementObject objram in ramMonitor.Get())
            {
                totalRam = Convert.ToDouble(objram["TotalVisibleMemorySize"]); 
                freeRam = Convert.ToDouble(objram["FreePhysicalMemory"]); 
                busyRam = totalRam - freeRam; 
            }

            // Выводим общий доступный объём памяти
            lblTotalRAMValue.Text = Convert.ToString(Convert.ToInt32(totalRam / (1024 * 1024))) + " ГБ";

            double RAM;

            if (lblUsedFreeRAM_usage)
            {
                lblUsedFreeRAM.Text = "Занято";
                prgUsedFreeRAM.Value = Convert.ToInt32(busyRam / totalRam * 100);
                RAM = busyRam; 
            }
            else
            {
                lblUsedFreeRAM.Text = "Свободно";
                prgUsedFreeRAM.Value = Convert.ToInt32(freeRam / totalRam * 100);
                RAM = freeRam;
            }

            if (lblUsedFreeRAMValue_percent)
            {
                lblUsedFreeRAMValue.Text = Convert.ToString(Convert.ToInt32(RAM / totalRam * 100)) + " %";
            }
            else
            {
                lblUsedFreeRAMValue.Text = Convert.ToString(Convert.ToInt32(RAM / (1024))) + " МБ";
            }


            ////// ЗАБИВАЕМ ПАМЯТЬ \\\\\\\

            // Заполняем память паттерном (если запущено заполнение и недостигнут зарезервированный порог памяти)
            if (fillingRAM && (freeRam / 1024 > Convert.ToDouble(nudReservedRAMValue.Value)))
            {

                // Если ещё много заполнять, то заполняем с указаной пользователям скоростью за секунду
                if (((freeRam / 1024) - Convert.ToDouble(nudReservedRAMValue.Value)) >= Convert.ToDouble(nudFillRateValue.Value))
                {
                    multiplier = Convert.ToInt32(nudFillRateValue.Value);
                }
                else // Если осталось заполнить меньше, чем шаг заполнения за секунду, то уменьшаем до единичного паттерна
                {
                    multiplier = 1;
                }
                ClassRAM.FillMemory();
            }
            else if (fillingRAM && (freeRam / 1024 <= Convert.ToDouble(nudReservedRAMValue.Value)))
            {
                btnPauseFillRAM.Enabled = false; 
                btnPauseFillRAM.BackColor = Color.LightGray;

                btnFreeRAM.Enabled = true; 
                btnFreeRAM.BackColor = Color.DeepSkyBlue;
            }

            // Отладка листа коллекций
            // decimal ListSize = ClassRAM.arrayList_Count;
            // lblPoolSize.Text = ListSize.ToString();

            int totalCPUUsage = 0; // Объявляем переменную для хранения текущей загрузки CPU

            // Забираем в цикле искомые значения загрузки ЦП
            foreach (ManagementObject objcpu in cpuMonitor.Get())
            {
                totalCPUUsage = Convert.ToInt32(objcpu["PercentProcessorTime"]); //общая загрузка ЦП
            }

            // Выводим загрузку ЦП на форму
            lblUsedCoreValue.Text = totalCPUUsage.ToString() + " %";
            prgUsedCore.Value = totalCPUUsage;

        }

        // Кнопка заполнить
        private void btnFillRAM_Click(object sender, EventArgs e)
        {
            fillingRAM = true; 

            btnFillRAM.Enabled = false; 
            btnFillRAM.BackColor = Color.LightGray; 

            btnPauseFillRAM.Enabled = true;
            btnPauseFillRAM.BackColor = Color.Yellow; 

            btnFreeRAM.Enabled = false; 
            btnFreeRAM.BackColor = Color.LightGray; 
        }

        // Кнопка пауза
        private void btnPauseFillRAM_Click(object sender, EventArgs e)
        {
            fillingRAM = false;

            btnFillRAM.Enabled = true; 
            btnFillRAM.BackColor = Color.Coral;

            btnPauseFillRAM.Enabled = false;
            btnPauseFillRAM.BackColor = Color.LightGray;

            btnFreeRAM.Enabled = true;
            btnFreeRAM.BackColor = Color.DeepSkyBlue;

        }

        // Кнопка Освободить
        private void btnFreeRAM_Click(object sender, EventArgs e)
        {
            ClassRAM.arrayList.Clear();

            // .......Вызов сборщика мусора..........
            // long totalMemory = GC.GetTotalMemory(false);

            GC.Collect(); 
            GC.WaitForPendingFinalizers();
            //......................................

            btnFreeRAM.Enabled = false; 
            btnFreeRAM.BackColor = Color.LightGray;

            btnFillRAM.Enabled = true; 
            btnFillRAM.BackColor = Color.Coral; 

            fillingRAM = false;
        }



        // Изменение числа зарезервированной памяти 
        private void nudReservedRAMValue_ValueChanged(object sender, EventArgs e)
        {
            if (nudReservedRAMValue.Value < 100)
            {
                nudReservedRAMValue.BackColor = Color.MistyRose;
            }
            else
            {
                nudReservedRAMValue.BackColor = Color.PaleGreen;
            }
        }

        // Изменение числа зарезервированных ядер
        private void nudReservedCoreValue_ValueChanged(object sender, EventArgs e)
        {
            if (nudReservedCoreValue.Value < 1)
            {
                nudReservedCoreValue.BackColor = Color.MistyRose;
            }
            else
            {
                nudReservedCoreValue.BackColor = Color.PaleGreen;
            }
        }



        // Кнопка Запустить стресс-тест
        private void btnStart_Click(object sender, EventArgs e)
        {
            btnStart.Enabled = false; 
            btnStart.BackColor = Color.LightGray;

            btnStop.Enabled = true; 
            btnStop.BackColor = Color.Lime; 

            // Забираем число тестипуемых ядер
            numberOfStressedCore = Environment.ProcessorCount - Convert.ToInt32(nudReservedCoreValue.Value);

            nudReservedCoreValue.Enabled = false;

            StartCPUStressTest(numberOfStressedCore);
        }

        // Кнопка Остановить стресс-тест
        private void btnStop_Click(object sender, EventArgs e)
        {
            StopCPUStressTest();

            btnStart.Enabled = true; 
            btnStart.BackColor = Color.Coral;

            btnStop.Enabled = false; 
            btnStop.BackColor = Color.LightGray; 

            nudReservedCoreValue.Enabled = true; 
        }


        private void nudStressRateValue_ValueChanged(object sender, EventArgs e)
        {
            powerFactorOfTest = Convert.ToInt32(nudStressRateValue.Value);
        }

        // Изменение скорости заполнения
        private void nudFillRateValue_ValueChanged(object sender, EventArgs e)
        {
            multiplier = Convert.ToInt32(nudFillRateValue.Value);
        }

        private void lblUsedCoreValue_Click(object sender, EventArgs e)
        {

        }

        // Запуск теста CPU
        public void StartCPUStressTest(int numberOfStressedCore)
        {
            threads = new Thread[numberOfStressedCore];
            decimal result = 0;

            for (int i = 0; i < numberOfStressedCore; ++i)
            {
                threads[i] = new Thread(() =>
                {
                    for (int j = 1; j < 700000000; j++)
                    {
                        result *= j;
                    }
                });
                threads[i].Start();
                threads[i].Priority = ThreadPriority.Lowest; // Ставим минимальный приоритет
            }

            foreach (Thread thread in threads)
            {
                thread.Join();
            }
        }

        // Остановка теста CPU
        public void StopCPUStressTest()
        {
            for (int i = 0; i < numberOfStressedCore; ++i)
            {
                threads[i].Abort();
            }
        }
    }
}