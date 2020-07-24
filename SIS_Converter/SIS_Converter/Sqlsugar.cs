using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlSugar;

namespace SIS_Converter
{
    public class Config
    {
        public static string GetCurrentProjectPath
        {

            get
            {
                return Environment.CurrentDirectory.Replace(@"\bin\Debug", @"\bin\Debug");
            }
        }
        public static string ConnectionString =@"DataSource="+ Environment.CurrentDirectory + @"\Data.db3";
        //public static string ConnectionString2 = @"DataSource=" + GetCurrentProjectPath + @"\DataBase\SqlSugar4xTest2.sqlite";
        //public static string ConnectionString3 = @"DataSource=" + GetCurrentProjectPath + @"\DataBase\SqlSugar4xTest3.sqlite";
    }

    public class DbContext
    {
        //如果实体类名称和表名不一致可以加上SugarTable特性指定表名
       
        //当然也支持自定义特性， 这里就不细讲了
        public DbContext()
        {
             Db = new SqlSugarClient(new ConnectionConfig()
            {
               
                ConnectionString = Config.ConnectionString,
                DbType = DbType.Sqlite,
                InitKeyType = InitKeyType.Attribute,
                IsAutoCloseConnection = true
                 
            });
           
        }
     
        //注意：不能写成静态的，不能写成静态的
        public SqlSugarClient Db;//用来处理事务多表查询和复杂的操作
        public SimpleClient<ErrorModel> ErrorDb { get { return new SimpleClient<ErrorModel>(Db); } }//用来处理Student表的常用操作
                                                                                                    // public SimpleClient<School> SchoolDb { get { return new SimpleClient<School>(Db); } }//用来处理School表的常用操作
      //  public SimpleClient<AssignmentModel> AssignmentDb { get { return new SimpleClient<AssignmentModel>(Db); } }//用来处理Student表的常用操作
       // public SimpleClient<StressModel> StressDb { get { return new SimpleClient<StressModel>(Db); } }//用来处理Student表的常用操作


    }
    //生成实体类

    //如果实体类名称和表名不一致可以加上SugarTable特性指定表名
    [SugarTable("Error")]
    public class ErrorModel
    {

        /*字段属性设置
         1.IsNullable表示表字段是否可空
         2.IsIgnore 为true表示不会生成字段到数据库
         3.IsIdentity表示为自增列
         4.IsPrimaryKey表示为主键
         5.Length 表示字段长度
         6.DecimalDigits 表示字段的精度 4.4
         7.ColumnDataType  强制设置数据库字段的类型（考虑到切换数据库有些类型其它库没有最新版本支持多个以逗号隔离，比如=“number,int”）*/


        [SugarColumn(IsNullable = false, IsPrimaryKey = true, IsIdentity = true)]
        public int ID { get; set; }

        [SugarColumn(IsNullable = true )]
        public String Content { get; set; }

        [SugarColumn(IsNullable = true)]
        public DateTime CreateTime { get; set; }

    }

    [SugarTable("Assignment")]
    //记录当前作业任务的情况的表
    public class AssignmentModel
    {

        /*字段属性设置
         1.IsNullable表示表字段是否可空
         2.IsIgnore 为true表示不会生成字段到数据库
         3.IsIdentity表示为自增列
         4.IsPrimaryKey表示为主键
         5.Length 表示字段长度
         6.DecimalDigits 表示字段的精度 4.4
         7.ColumnDataType  强制设置数据库字段的类型（考虑到切换数据库有些类型其它库没有最新版本支持多个以逗号隔离，比如=“number,int”）*/


        [SugarColumn(IsNullable = false, IsPrimaryKey = true, IsIdentity = false)]
        public string ID { get; set; }        
        [SugarColumn(IsNullable = true)]
        public String Rack { get; set; }
        [SugarColumn(IsNullable = true)]
        public String X_Num { get; set; }
        [SugarColumn(IsNullable = true)]
        public String Y_Num { get; set; }
        [SugarColumn(IsNullable = true)]
        public String Depth { get; set; }     
        [SugarColumn(IsNullable = true)]
        public String Status { get; set; }
        [SugarColumn(IsNullable = true)]       
        public String Name { get; set; }      
        [SugarColumn(IsNullable = true)]
        public String Code { get; set; }
        [SugarColumn(IsNullable = true)]
        public DateTime CreateTime { get; set; }
       
    }

   
    [SugarTable("User")]
    public class UserModel
    {

        /*字段属性设置
         1.IsNullable表示表字段是否可空
         2.IsIgnore 为true表示不会生成字段到数据库
         3.IsIdentity表示为自增列
         4.IsPrimaryKey表示为主键
         5.Length 表示字段长度
         6.DecimalDigits 表示字段的精度 4.4
         7.ColumnDataType  强制设置数据库字段的类型（考虑到切换数据库有些类型其它库没有最新版本支持多个以逗号隔离，比如=“number,int”）*/


        [SugarColumn(IsNullable = false, IsPrimaryKey = true, IsIdentity = true)]
        public int ID { get; set; }

        [SugarColumn(IsNullable = true)]
        public String User { get; set; }
        public String Password { get; set; }

        [SugarColumn(IsNullable = true)]
        public DateTime CreateTime { get; set; }

    }










    public class DemoManager : DbContext//继承DbContext
    {
        //  public ErrorModel insertObj { get; private set; }

        //SimpleClient实现查询例子
       

        //插入例子
        public void InitTable()
        {
            Db.CodeFirst.InitTables(typeof(ErrorModel));
            Db.CodeFirst.InitTables(typeof(AssignmentModel));
            Db.CodeFirst.InitTables(typeof(UserModel));

        }

       


    }

}
