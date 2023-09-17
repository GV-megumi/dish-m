using System;
using static System.Console;
using System.Text.RegularExpressions;


namespace caidan
{

    class TableType
    {




        //输出表格

        /*
        用于输出表格
        获取表格和 该表每列最长的字符串的长度（用于对其）
        用foreach输出
        遇到未初始化的string退出

        */
        public int putTable(string[,] aaa, int[] strlens)
        {

            if (aaa == null)
                return 1;
            int i = 0, j = 1;
            int spacenum;//空格数
            int number = 0; //记录str里的数字/字母数

            int num = aaa.GetLength(1);

            Write("行号    ");
            foreach (string s in aaa)
            {
                
                if (s == null||s==" ")
                    break;


                if (i == num)
                {
                    i = 0;
                    WriteLine();
                    Write($"{j:D3}     ");
                    j++;
                }
                Write(s);

                number = putspace(s);
                spacenum = strlens[i] - s.Length;
                while (number >= 0)
                {
                    Write(" ");
                    number--;
                }


                //Write(spacenum);
                while (spacenum >= 0)
                {
                    spacenum--;
                    Write("  ");
                    //pause();
                }
                i++;
                //WriteLine("qweeqewee");


            }

            WriteLine("\n\n");

            return 0 - j + 1;


        }





        //返回子符串中的数字和字母以及标点符号的数量（不包含中文字符）
        /*
        辅助表格对齐
        中文占两个字符位
        所以需要判断非中文的个数，在输出后补上相应数量的空格
        */
        public int putspace(string inputString)
        {
            // 使用正则表达式匹配包含字母、数字和标点符号的子串，排除中文字符
            string pattern = @"[a-zA-Z0-9\p{P}]+";
            MatchCollection matches = Regex.Matches(inputString, pattern);

            int letterDigitAndPunctuationCount = 0;

            foreach (Match match in matches)
            {
                letterDigitAndPunctuationCount += match.Value.Length;
            }
            return letterDigitAndPunctuationCount;
        }




        public void pause()
        {
            WriteLine("\n按任意键继续");
            ReadKey();
        }
    }
}