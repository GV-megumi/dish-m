using System;
using System.IO;
using static System.Console;
using MySql.Data.MySqlClient;
using System.Text;


namespace caidan
{
    class MyFoodSql
    {

        string connectionString;//数据库连接
        string passwd;//密码
        string[][] sqlHand = new string[5][];//记录mysql各表属性名

        public MyFoodSql(ShiCai shiCai)
        {
            connectionString = "Server=localhost;Port=3306;Database=food;Uid=root;Pwd=;";
            passwd = "";

            sqlHand[0] = new string[] { "name", "P_C", "I_N_D", "S_C", "NUTR" };
            sqlHand[1] = new string[] { "NAME", "date", "nc", "sl", "number", "M_F" };
            sqlHand[2] = new string[] { "NAME", "M_F", "poo" };
            sqlHand[3] = new string[] { "M_F", "address", "FPLN", "PSN" };
            sqlHand[4] = new string[] { "name", "M_I", "S_S", "R_S", "R_C", "M_V" };


            //订阅事件
            shiCai.SqlDelete += Delete;
            shiCai.SqlInsert += Insert;
            shiCai.SqlUpdate += Update;
            shiCai.SqlGetShicaiTable += SqlGetShicai;
            shiCai.SqlGetCaipuTable += SqlGetCaipu;


        }

        //检查是否有单引号
        /*
        用于检查数据，防止sql语句出错
        防止用户输入多空格 
        若用户输入单引号，需要再加一个单引号（mysql转义）
        若数据为char型，则需用''括起来，若为int型则不需要
        */

        string CheckData(string data, bool isint)
        {

            //对int的特殊照顾
            if (isint)
            {
                return data;
            }


            StringBuilder result = new StringBuilder();

            foreach (char c in data)
            {
                // 如果当前字符是单引号，将其后面再添加一个单引号
                if (c == '\'')
                {
                    result.Append("''");
                }
                else if (c == ' ')
                {
                    continue;
                }
                else
                {
                    result.Append(c);
                }
            }

            return "'" + result.ToString() + "'";
        }





        /*
        相应shicai的Do事件
        将传递的数据进行处理并向mysql发送对应语句
        如果错误则输出错误语句并作出一定解释
        向shicai返回修改结果（true/false）
        */
        bool Update(string[] str, int num)
        {
            int column, columnnum; //要修改的列号，总列数
            columnnum = str.Length - 1;
            column = int.Parse(str[columnnum]);

            // string updateQuery = "UPDATE yourTableName SET columnName = newValue WHERE yourCondition"
            // 要执行的更新操作 SQL 语句
            string updateQuery = "UPDATE ";
            string data;

            //判断sql属性类型是否为int
            data = CheckData(str[column], num == 1 && column == 4);
            switch (num)
            {
                case 0:
                    //食材
                    updateQuery += " food_i SET ";

                    break;
                case 1:
                    //库存
                    updateQuery += " inv SET ";
                    break;
                case 2:
                    //进货渠道
                    updateQuery += " sog SET ";
                    break;
                case 3:
                    //商
                    updateQuery += " sup SET ";
                    break;
                case 4:
                    updateQuery += " rmm SET ";
                    break;

            }




            updateQuery += sqlHand[num][column] + " = " + data + " where ";

            for (int i = 0; i < columnnum; i++)
            {
                if ((i == column)//更新行跳过where
                || (num == 1 && (i == 0 || i == 5))//对库存的特殊照顾
                )
                    continue;

                //  后一项为数据是int的条件（num == 1 && i == 4  时 data为一个int，不需要加单引号
                data = CheckData(str[i], num == 1 && i == 4);



                updateQuery += " " + sqlHand[num][i] + " = " + data + " and";


            }

            updateQuery = updateQuery.Substring(0, updateQuery.Length - 3);//减去最后一个and

            //对库存的特殊照顾
            if (num == 1)
            {
                updateQuery += " and sog_id in ( SELECT DISTINCT subquery.sog_id FROM ( SELECT inv.sog_id FROM inv, sog WHERE inv.sog_id = sog.SOG_ID AND sog.NAME = '" + str[0] + "' AND sog.M_F = '" + str[5] + "' ) AS subquery ) ;";
            }

            //WriteLine(updateQuery);
            //ReadKey();



            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    using (MySqlCommand command = new MySqlCommand(updateQuery, connection))
                    {
                        int rowsAffected = command.ExecuteNonQuery();

                        Console.WriteLine($"更新了 {rowsAffected} 行数据.");
                    }

                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                WriteLine("错误，请检查更新后是否有重复行/食材是否存在/供应商是否存在/其他错误");
                Console.WriteLine("Error: " + ex.Message);
                return false;
            }







            return true;

        }



        bool Delete(string[] s, int num)
        {
            //deleteQuery = "DELETE FROM yourTableName WHERE yourCondition";
            string deleteQuery = "DELETE FROM ";

            // 要执行的删除操作 SQL 语句
            switch (num)
            {
                case 0:
                    //食材
                    deleteQuery += " food_i WHERE `name`= " + CheckData(s[0], false) + ";";

                    break;
                case 1:
                    //库存
                    deleteQuery += " inv WHERE `date` = "
                    + CheckData(s[1], false) + " AND `sog_id` IN ("
                    + "SELECT * FROM (SELECT inv.sog_id FROM inv, sog WHERE inv.sog_id = sog.SOG_ID AND "
                    + "sog.NAME = " + CheckData(s[0], false) + " AND sog.M_F = "
                    + CheckData(s[5], false) + ") AS subquery);";
                    break;
                case 2:
                    //进货渠道
                    //sourceOfGoods[0, 0] = "品名";
                    //sourceOfGoods[0, 1] = "制造商";
                    deleteQuery += " sog WHERE `NAME`= " + CheckData(s[0], false) + " AND `M_F` = "
                     + CheckData(s[1], false) + ";";
                    break;
                case 3:
                    //商
                    deleteQuery += " sup WHERE `M_F`= " + CheckData(s[0], false) + ";";
                    break;
                case 4:
                    //商
                    deleteQuery += " rmm WHERE `name`= " + CheckData(s[0], false) + ";";
                    break;
            }


            try
            {

                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();


                    using (MySqlCommand command = new MySqlCommand(deleteQuery, connection))
                    {
                        int rowsAffected = command.ExecuteNonQuery();

                        Console.WriteLine($"删除了 {rowsAffected} 行数据.");
                    }

                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                WriteLine("失败：请先检查库存或进货渠道是否还有该食材/进货商");
                WriteLine("Error: " + ex.Message);

                return false;
            }



            //ReadKey();


            return true;
        }






        bool Insert(string[] str, int num)
        {


            string insertQuery = "INSERT INTO "; ;

            // 要执行操作 SQL 语句
            switch (num)
            {
                case 0:
                    insertQuery += "`food_i` (`name`,`P_C`,`I_N_D`,`S_C`,`NUTR`) VALUES (";
                    //食材
                    break;

                case 1:
                    //库存
                    /*
                    INSERT INTO 
                    `inv` (  `sog_id`,`date`,`nc`,`sl`,`number`) VALUES (
                    (SELECT SOG_ID FROM sog WHERE 
                    sog.NAME = 'zx' AND sog.M_F = 'zx'),'2023.01','12','12',12);
                    */


                    insertQuery += "`inv` (  `sog_id`,`date`,`nc`,`sl`,`number`) VALUES ("
                    + "(SELECT SOG_ID FROM sog WHERE sog.NAME = "
                    + CheckData(str[0], false)
                    + " AND sog.M_F = "
                    + CheckData(str[5], false) + "),"
                    + CheckData(str[1], false) + ","
                    + CheckData(str[2], false) + ","
                    + CheckData(str[3], false) + ","
                    + CheckData(str[4], true) + ")";
                    goto link;

                case 2:
                    //进货渠道

                    insertQuery += "`SOG` (`NAME`,`M_F`,`poo`)  VALUES (";
                    break;
                case 3:
                    //商
                    insertQuery += "`SUP` (`M_F`,`address`,`FPLN`,`PSN`)  VALUES (";
                    break;

                case 4:
                    //菜谱
                    insertQuery += "`rmm` (  `name` ,`M_I`,`S_S` ,`R_S` ,`R_C`,`M_V`)  VALUES (";
                    break;
            }


            //添加值
            foreach (string s in str)
            {
                insertQuery += "" + CheckData(s, false) + ",";
            }
            insertQuery = insertQuery.Substring(0, insertQuery.Length - 1);
            insertQuery += ")";



        link:

            try
            {

                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();


                    using (MySqlCommand command = new MySqlCommand(insertQuery, connection))
                    {
                        int rowsAffected = command.ExecuteNonQuery();

                        Console.WriteLine($"添加了了 {rowsAffected} 行数据.");
                    }

                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                WriteLine("错误：请检查是否有该食材或供应商/数量请输入数字");
                WriteLine("Error: " + ex.Message);

                return false;
            }



            return true;
        }





        /*将食材的四个表信息全部写入内存，同时记录每行
        */
        bool SqlGetShicai(
            string[,] shicai, ref int shicainum,
            string[,] supplier, ref int supplierNum,
            string[,] sourceOfGoods, ref int sogNum,
            string[,] inventory, ref int inventoryNum,
            int[][] strlens)
        {

            // 连接字符串，根据您的数据库配置进行修改

            string query;
            bool back = false;

        backk:
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    // 执行查询
                    WriteLine("正在加载食材相关数据：");
                    query = "SELECT * FROM food_i";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                shicainum++;




                                // 从结果集中获取数据并处理


                                for (int i = 0; i < 5; i++)
                                {
                                    shicai[shicainum, i] = reader.GetString(sqlHand[0][i]);
                                    if (shicai[shicainum, i].Length > strlens[0][i])
                                    {
                                        strlens[0][i] = shicai[shicainum, i].Length;
                                    }
                                }

                            }
                        }
                    }
                    WriteLine("食材数据加载完成");





                    WriteLine("正在加载供应商相关数据：");
                    query = "SELECT * FROM SUP";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                supplierNum++;




                                // 从结果集中获取数据并处理

                                for (int i = 0; i < 4; i++)
                                {
                                    supplier[supplierNum, i] = reader.GetString(sqlHand[3][i]);
                                    if (supplier[supplierNum, i].Length > strlens[3][i])
                                    {
                                        strlens[3][i] = supplier[supplierNum, i].Length;
                                    }
                                }

                            }
                        }
                    }
                    WriteLine("供应商数据加载完成");




                    //sourceOfGoods  sogNum



                    WriteLine("正在加载进货渠道相关数据：");
                    query = "SELECT * FROM SOG";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                sogNum++;



                                // 从结果集中获取数据并处理

  

                                for (int i = 0; i < 3; i++)
                                {
                                    sourceOfGoods[sogNum, i] = reader.GetString(sqlHand[2][i]);
                                    if (sourceOfGoods[sogNum, i].Length > strlens[2][i])
                                    {
                                        strlens[2][i] = sourceOfGoods[sogNum, i].Length;
                                    }
                                }

                            }
                        }
                    }
                    WriteLine("进货渠道数据加载完成");






                    //inventory    inventoryNum   1



                    WriteLine("正在加载库存相关数据：");
                    query = "SELECT * FROM SOG,inv where SOG.SOG_ID=inv.sog_id";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                inventoryNum++;
                                // 从结果集中获取数据并处理
                                for (int i = 0; i < 6; i++)
                                {
                                    inventory[inventoryNum, i] = reader.GetString(sqlHand[1][i]);

                                    if (inventory[inventoryNum, i].Length > strlens[1][i])
                                    {
                                        strlens[1][i] = inventory[inventoryNum, i].Length;
                                    }
                                }

                            }
                        }
                    }
                    WriteLine("库存数据加载完成");












                    connection.Close();
                    //putfood_i();

                    Clear();


                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                if (ex.Message.Contains("Access denied"))
                {
                    if (!back)
                    {
                        Write("请尝试输入数据库密码以解决：");
                        passwd = ReadLine() ?? "";
                        connectionString = "Server=localhost;Port=3306;Database=food;Uid=root;Pwd=" + passwd + ";";
                        back = true;
                        goto backk;

                    }
                    return false;
                }

                // Unknown database 'food'

                if (ex.Message.Contains("Unknown database 'food'"))
                {
                    Write("错误：没有该数据库，按任意键以新建");
                    ReadKey();
                    createDatabase();
                    goto backk;


                }
                ReadKey();



            }
            return true;
        }

        void createDatabase()
        {

            string connectionString = "Server=localhost;Port=3306;Uid=root;Pwd=" + passwd + ";";

            // 创建 MySQL 连接对象
            MySqlConnection connection = new MySqlConnection(connectionString);

            try
            {
                // 打开数据库连接
                connection.Open();

                // 创建一个新的数据库 food
                string createDatabaseQuery = "drop database if exists food ; CREATE DATABASE food;";
                MySqlCommand cmd = new MySqlCommand(createDatabaseQuery, connection);
                cmd.ExecuteNonQuery();

                WriteLine("数据库 food 创建成功！");



                // 获取程序目录下的 SQL 脚本文件路径
                string scriptFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "food.sql");


                while (!File.Exists(scriptFilePath))
                {
                    WriteLine("错误，未找到指定脚本，请将：food.sql 放到以下文件夹中 ：");
                    WriteLine(AppDomain.CurrentDomain.BaseDirectory + "\n按任意键继续");
                    ReadKey();
                    Clear();

                }

                // 读取 SQL 脚本内容
                string scriptContent = "use food;" + File.ReadAllText(scriptFilePath);

                cmd = new MySqlCommand(scriptContent, connection);

                // 执行 SQL 命令
                cmd.ExecuteNonQuery();

                WriteLine("SQL 脚本执行成功！");














            }
            catch (Exception ex)
            {
                Console.WriteLine("创建数据库时出现错误：" + ex.Message);
            }
            finally
            {
                // 关闭数据库连接
                connection.Close();
            }








        }




        bool SqlGetCaipu(string[,] caiPU, ref int Caipunum, int[] strlens)
        {





            string query;


            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    // 执行查询
                    WriteLine("正在加载菜谱相关数据：");
                    query = "SELECT * FROM rmm";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Caipunum++;

                                for (int i = 0; i < 6; i++)
                                {
                                    // 从结果集中获取数据并处理
                                    caiPU[Caipunum, i] = reader.GetString(sqlHand[4][i]);
                                    if (caiPU[Caipunum, i].Length > strlens[i])
                                    {
                                        strlens[i] = caiPU[Caipunum, i].Length;
                                    }
                                }

                            }
                        }
                    }
                    WriteLine("菜谱数据加载完成");







                    connection.Close();
                    //putfood_i();

                    Clear();


                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                if (ex.Message.Contains("Access denied"))
                {
                    return false;
                }
                ReadKey();

            }
            return true;


        }

    }


}