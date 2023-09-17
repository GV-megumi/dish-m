using System;
using System.Threading.Tasks;
using static System.Console;

namespace caidan
{

    class UI
    {
        //输入按键
        ConsoleKey key;
        //表格


        int tabnum;

        //自定义委托声明，用于输出表格
        public delegate void MyDel1<T>(ref T a);

        //非标准事件委托，用于发布增删改事件
        public delegate T MyDel2<T, R>(R a, int tablenum);

        //输出表格
        public event MyDel1<int>? putTable;
        public event MyDel1<bool>? putCaidan;








        void mainui()
        {

            WriteLine("Q:食材管理");
            WriteLine("W:菜谱管理");
            WriteLine("E:菜单");
            WriteLine("R:退出");
            Write("请键入字母选择：");
        backk:
            key = Console.ReadKey(intercept: true).Key;
            if (key == ConsoleKey.Q)
            {
                Clear();
                shicaiui();
            }
            else if (key == ConsoleKey.W)
            {
                Clear();
                tabnum = 4;
                IDCUI();
            }
            else if (key == ConsoleKey.E)
                CaidanUi();
            else if (key == ConsoleKey.R)
                return;
            else
            {
                goto backk;
            }

        }



        void CaidanUi()
        {

            Clear();


            WriteLine("Q:荤菜");
            WriteLine("W:素菜");
            WriteLine("E:返回");
            Write("请键入字母选择：");
            bool isVegetables;
        backk:
            key = Console.ReadKey(intercept: true).Key;
            if (key == ConsoleKey.Q)
            {
                isVegetables = false;

            }
            else if (key == ConsoleKey.W)
            {
                isVegetables = true;

            }
            else if (key == ConsoleKey.E)
            {
                goto backkk;
            }
            else
            {
                goto backk;
            }

            putCaidan?.Invoke(ref isVegetables);

            WriteLine("按任意键返回");
            ReadKey();

        backkk:
            Clear();
            mainui();

        }

        void shicaiui()
        {

            WriteLine("Q:食材管理");
            WriteLine("W:库存管理");
            WriteLine("E:进货渠道管理：");
            WriteLine("R:供应商管理");
            WriteLine("T:返回");
            Write("请键入字母选择：");
        backk:
            key = Console.ReadKey(intercept: true).Key;



            if (key == ConsoleKey.Q)
                tabnum = 0;
            else if (key == ConsoleKey.W)
                tabnum = 1;
            else if (key == ConsoleKey.E)
                tabnum = 2;
            else if (key == ConsoleKey.R)
                tabnum = 3;
            else if (key == ConsoleKey.T)
            {
                Clear();
                mainui();
                return;
            }
            else
            {
                goto backk;
            }


            IDCUI();


        }

        void IDCUI()
        {
            Clear();
            int rowmax = tabnum;
            putTable?.Invoke(ref rowmax);
            rowmax = 0 - rowmax;

            if (rowmax == -1)
            {
                WriteLine("错误：未获取到表格,将返回到主菜单");
                pause();
                mainui();
                return;
            }

            WriteLine("Q:添加");
            WriteLine("W:删除");
            WriteLine("E:修改");
            WriteLine("R:返回");
            Write("请键入按键完成对应操作：");

        backk:
            key = Console.ReadKey(intercept: true).Key;

            if (key == ConsoleKey.Q)
                Insert();
            else if (key == ConsoleKey.W)
                Delete();
            else if (key == ConsoleKey.E)
                Update();
            else if (key == ConsoleKey.R)
            {
                Clear();
                if (tabnum == 4)
                    mainui();
                else
                    shicaiui();
            }
            else
            {
                //Clear();
                goto backk;
            }


        }






        //更新事件
        public event MyDel2<bool, string[]>? CallUpdate;
        //更新函数 
        /*
        shicai的DoUpdate订阅了事件CallUpdate，当该事件被发布时，会自动调用食材的DoUpdat
        先调用puttable事件获取要更新的表的总行数，
        然后调用Gettablehand事件获取表头，此时也知道了该表的列数，

        让用户输入行和列，以选中要更新的数据的位置
        此处会做出行列限制，以免输入错误
        然后发布CallUpdate事件，向shicai传递一个
        updateData数组，记录了要修改的值的位置及修改内容
        还有一个int数，用于选择修改的表是哪个。
        callUpdate事件返回一个bool类型，用以判断是否修改成功
        对更新的剩下操作会交给shicai的DoUpdate和sql的Sqlupdate去做
        */
        void Update()
        {

            int row, column, rowmax = tabnum, i;//行号，列号,总行数
            string[]? hand, updateData = new string[3];



            //获取行
            Clear();

            putTable?.Invoke(ref rowmax);
            rowmax = 0 - rowmax;
            if (rowmax == -1)
            {
                WriteLine("错误：未获取到表格,将返回到主菜单");
                pause();
                mainui();
                return;
            }
        backk:
            Write("请输入要修改的行号并按回车确定：");
            try
            {

                row = int.Parse(ReadLine() ?? "-999");

            }
            catch (FormatException)
            {
                goto backk;
            }
            catch (Exception ex)
            {
                WriteLine("发生异常" + ex.Message);
                pause();
                Update();
                return;
            }


            if (row > rowmax || row < 0)
            {
                //Clear();
                Write("错误：没有该行。   ");
                goto backk;
            }







            //获取表头
            Clear();
            //gettablehand只需传递一个参数tabnum，这里有两个参数是不想为该事件单独声明委托，  第一个参数-1没有任何意义
            if ((hand = GetTableHand?.Invoke(-1, tabnum)) == null)
            {
                WriteLine("ERROR:未获取到表头");
                pause();
                goto f1;
            }

            i = 0;
            foreach (string s in hand)
            {
                WriteLine($"{i:D3}:  {s}");
                i++;
            }


        //获取列
        backkk:
            Write("请输入要修改的列前面的标号并按回车确定：");
            try
            {

                column = int.Parse(ReadLine() ?? "-99");

            }
            catch (FormatException)
            {
                goto backkk;
            }
            catch (Exception ex)
            {
                Write("发生异常" + ex.Message);
                pause();
                Update();

                return;
            }

            //对库存表的单独照顾：库存不可以改 食材名和供应商，
            if (tabnum == 1 && (column == 0 || column == 5))
            {
                WriteLine("此项不可更改");
                pause();
                goto f1;

            }




            if (column >= hand.Length || column < 0)
            {
                //Clear();
                Write("错误：没有该列。  ");
                goto backkk;
            }


            WriteLine($"[{row},{column}]");


            //记录行列数值
            updateData[0] = row.ToString();
            updateData[1] = column.ToString();



            //获取修改的内容
            Clear();
            rowmax = tabnum;
            putTable?.Invoke(ref rowmax);
            rowmax = 0 - rowmax;
            if (rowmax == -1)
            {
                WriteLine("错误：未获取到表格,将返回到主菜单");
                pause();
                mainui();
                return;
            }
            Write($"将第{row:D3}的  {hand[column]}  改为：");
            try
            {
                updateData[2] = ReadLine() ?? " ";
            }
            catch (Exception ex)
            {
                WriteLine("发生异常" + ex.Message);
                pause();
                Update();

                return;
            }








            Clear();
            if (CallUpdate?.Invoke(updateData, tabnum) ?? false)
            {
                WriteLine("修改成功");

            }
            else
            {
                WriteLine("修改失败");

            }
            //Thread.Sleep(500);
             sleep(500).Wait();
            


        f1:
            Clear();
            IDCUI();


        }

        static async Task sleep(int n)
        {
            await Task.Delay(n);
            
        }




        //删除事件
        public event MyDel2<bool, int>? CallDelete;
        /*
        与update类似：获取要删除的行，发布Calldelete事件

        calldelete向Dodelete传递两个int
        一个所删除的行
        一个所选择的表
        返回类型为bool
        */

        void Delete()
        {
            int row, rowmax = tabnum;
            Clear();
            putTable?.Invoke(ref rowmax);
            rowmax = 0 - rowmax;

            if (rowmax == -1)
            {
                WriteLine("错误：未获取到表格,将返回到主菜单");
                pause();
                mainui();
                return;
            }
        backk:
            Write("请输入要删除的行号并按回车确定：");
            try
            {

                row = int.Parse(ReadLine() ?? "-1");

            }
            catch (FormatException)
            {
                goto backk;
            }
            catch (Exception ex)
            {
                WriteLine("发生异常" + ex.Message);
                pause();
                Delete();

                return;
            }

            if (row > rowmax || row < 0)
            {
                //Clear();
                Write("错误：没有该行。   ");
                goto backk;
            }



            WriteLine(row);



            Clear();
            if (CallDelete?.Invoke(row, tabnum) ?? false)
            {
                WriteLine("删除成功");

            }
            else
            {
                WriteLine("删除失败");

            }
            //Thread.Sleep(500);
            sleep(500).Wait();
            Clear();
            IDCUI();

        }

        //事件，用于获取表头
        public event MyDel2<string[], int>? GetTableHand;




        //事件，用于发布增加
        public event MyDel2<bool, string[]>? CallInsert;

        /*
        与Update类似：获取要增加的信息，发布Calldelete事件
        不同之处：
        insert需要单独获取对应表格的表头，以便用户知道该输入什么信息
        所以需要事先发布GetTableHand事件，
        该事件传递两个参数
        第一个参数无意义，只因声明该事件所用到的委托需要传递两个参数所以存在
        第二个参数为int，用于选中表格
        该事件返回一个string[]类，存储了表头

        用户输入具体列的值时，会直接替换掉表头对应列的值，（可以少一个string[]的声明）
        Callinsert
        传递两个参数：
        一个string[]用于记录增加内容
        一个int用于选表
        返回bool类型
        */

        void Insert()
        {

            Clear();
            //putTable(tabnum);

            string[]? hand;
            if ((hand = GetTableHand?.Invoke(-1, tabnum)) == null)
            {
                WriteLine("ERROR:未获取到表头");
                pause();
                goto f1;
            }


            for (int i = 0; i < hand.Length; i++)
            {
            backk:
                try
                {

                    Write($"请输入 {hand[i]}:");
                    hand[i] = ReadLine() ?? " ";


                }
                catch (FormatException)
                {
                    goto backk;
                }

                catch (Exception ex)
                {
                    WriteLine("发生异常" + ex.Message);
                    pause();
                    Insert();

                    return;
                }
            }

            foreach (string s in hand)
            {
                WriteLine(s);
            }
            //ReadKey();


            Clear();
            if (CallInsert?.Invoke(hand, tabnum) ?? false)
            {
                WriteLine("添加成功");

            }
            else
            {
                WriteLine("添加失败");

            }
            //Thread.Sleep(500);
            sleep(500).Wait();
        f1:
            Clear();
            IDCUI();


        }

































        public delegate bool Link();
        public event Link? LinkToSql;




        public void run()
        {
            if (!LinkToSql?.Invoke() ?? false)
            {
                ForegroundColor = ConsoleColor.Red;
                WriteLine("\n\n\n发生错误,未获取到相关信息：连接数据库失败");
                ResetColor(); // 重置颜色为默认
                pause();
                Clear();
            }
            else
                mainui();


        }

        public void pause()
        {
            WriteLine("\n按任意键继续");
            ReadKey();
        }


    }
}