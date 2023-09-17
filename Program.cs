
using System;
using static System.Console;
using System.IO;

using caidan;


namespace asss
{

    class Program
    {

        static void Main()
        {



 


            UI caiDanUI = new();
            ShiCai shiCai = new(caiDanUI);
            MyFoodSql myFoodSql = new(shiCai);

           





            Clear();


            caiDanUI.run();

            //caiDanUI.pause();



        }
    }

}