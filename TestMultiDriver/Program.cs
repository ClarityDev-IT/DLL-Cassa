using DemoMultiDriver;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace TestMultiDriver
{
    static class Program
    {
        /// <summary>
        /// Punto di ingresso principale dell'applicazione.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new Form1());

            Stampa s = new Stampa();

            //s.open("172.16.10.57", "23");
            //s.button6_Click();
            //s.close();
            //s.Dispose();
            s.open("172.16.10.57", "23");

            for (int i = 0; i <= 3; i++)
            {

                //s.button6_Click();

                s.message("=K");
                s.message("=D1/(Messaggio1");
                s.message("=R1/$500");
                s.message("=T");

                //s.Dispose();

            }

            s.close();
        }
    }
}