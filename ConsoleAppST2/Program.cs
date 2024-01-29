using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Runtime.InteropServices.WindowsRuntime;

namespace ConsoleAppST2
{
    internal class Program
    {
        //string connectionString = "Data Source = THINKPAD\\SQLEXPRESS;" +
        //                            "Initial Catalog = QLSV2;" +
        //                            "Integrated Security = True ";
        
        static void Main(string[] args)
        {
            string connecionString = ConfigurationManager
                            .ConnectionStrings["QLSV_connectionString"]
                            .ConnectionString;
            string sMaDA, sTenDA, sDiaDiem;
            int lc = 0;

            while (true)
            {
                Console.WriteLine("------------------MENU--------------------------");
                Console.WriteLine("1. THÊM DỰ ÁN MỚI");
                Console.WriteLine("2. HIỆN DANH SÁCH DỰ ÁN");
                Console.WriteLine("3. CHỈNH SỬA DỰ ÁN THEO MÃ DỰ ÁN");
                Console.WriteLine("4. XÓA DỰ ÁN THEO MÃ DỰ ÁN");
                Console.WriteLine("0. THOÁT");
                Console.WriteLine("-----------------------------------------------");
                Console.Write("MỜI BẠN CHỌN: ");
                

                lc = Convert.ToInt32(Console.ReadLine());

                switch (lc)
                {
                    case 1:
                        Console.Write("Nhap ma du an: ");
                        sMaDA = Console.ReadLine();
                        // kiểm tra xem có bị trùng lặp khóa chính không
                        while (KiemTraKhoaChinh_DuAn(connecionString, sMaDA) == false)
                        {
                            Console.Write("Nhap ma du an: ");
                            sMaDA = Console.ReadLine();
                        }

                        Console.Write("Nhap ten du an: ");
                        sTenDA = Console.ReadLine();

                        Console.Write("Nhap dia diem: ");
                        sDiaDiem = Console.ReadLine();

                        bool i = ThemSinhVien(connecionString, sMaDA, sTenDA, sDiaDiem);

                        if (i)
                        {
                            Console.WriteLine("Them thanh cong");
                        }
                        else
                        {
                            Console.WriteLine("Them khong thanh cong");
                        }
                        break;

                        case 2:
                            hienThiDanhSachDuAn(connecionString);
                            break;
                    case 3:
                        string yes = "y";
                        string no = "n";
                        string check;
                        Console.WriteLine("Bạn có muốn chỉnh sửa dữ liệu của Bảng Dự Án không ? [ y - có, n - không]");
                        check = Console.ReadLine();
                        if (check == yes)
                        {
                            Console.WriteLine("bạn đã đồng ý chỉnh sửa");
                            Console.Write("Nhập mã dự án bạn muốn chỉnh sửa: ");
                            sMaDA = Console.ReadLine();
                            // kiểm tra xem có tồn tại khóa chính không ?
                            while(KiemTraKhoaChinh_DuAn(connecionString, sMaDA) == true)
                            {
                                Console.WriteLine("mã dự án không tồn tại");
                                Console.Write("Nhập mã dự án bạn muốn chỉnh sửa: ");
                                sMaDA = Console.ReadLine();
                            }
                            Console.Write("Nhập tên dự án mới: ");
                            sTenDA = Console.ReadLine();
                            Console.Write("Nhập địa điểm mới: ");
                            sDiaDiem = Console.ReadLine();

                            chinhSuaDanhSachDuAn(connecionString, sMaDA, sTenDA, sDiaDiem);
                        }
                        else if(check == no)
                        {
                            Console.WriteLine();
                        }
                        break;

                    case 4:
                        Console.Write("Nhập mã dự án bạn muốn xóa: ");
                        sMaDA = Console.ReadLine();
                        while (KiemTraKhoaChinh_DuAn(connecionString, sMaDA) == true)
                        {
                            Console.WriteLine("mã dự án không tồn tại");
                            Console.Write("Nhập mã dự án bạn muốn chỉnh sửa");
                            sMaDA = Console.ReadLine();
                        }
                        xoaDuAnTheoMaDuAn(connecionString, sMaDA);
                        break;

                    case 0:
                        return;
                }

            }
             Console.ReadKey();
        }

        private static bool ThemSinhVien(string connecionString, string sMaDA, string sTenDA, String sDiaDiem)
        {
            //string insert_command = "INSERT INTO DU_AN (sMaDA, sTenDA, sDiaDiem) " +
            //                        "VALUES ('"+sMaDA+"', '"+ sTenDA + "','"+sDiaDiem+ "')";
            String insert_proc = "INSERT_DU_AN";
            using (SqlConnection conn = new SqlConnection(connecionString))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    //cmd.CommandType = CommandType.Text;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = insert_proc;
                    // khởi tạo và truyền các con số
                    //cmd.Parameters.Add("@sMaDa", SqlDbType.VarChar, 10).Value = sMaDA;
                    cmd.Parameters.AddWithValue("@sMaDa", sMaDA);
                    cmd.Parameters.AddWithValue("@sTenDa", sTenDA);
                    cmd.Parameters.AddWithValue("@sDiaDiem", sDiaDiem);
                    conn.Open();
                    int i = cmd.ExecuteNonQuery();
                    conn.Close();

                    return (i > 0);
                }
            }
        }

        private static bool KiemTraKhoaChinh_DuAn (string connection, string maDa)
        {
            string select_proc = "KiemTraKhoaChinh_tblDU_AN";
            using (SqlConnection conn = new SqlConnection())
            {
                conn.ConnectionString = connection;
                using(SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = select_proc;
                    cmd.CommandType = CommandType.StoredProcedure;
                    // khoi tao doi tuong 
                    cmd.Parameters.AddWithValue("@sMaDa", maDa);

                    conn.Open();
                    bool i = (cmd.ExecuteScalar() != null); // true -> ma da da ton tai
                    conn.Close();
                    if(i)
                    {
                        //Console.WriteLine("Ma du an: " + maDa + "da ton tai 🤡🤡🤡");
                        return false;
                    }
                    return true;
                }

            }    
        }


        private static void hienThiDanhSachDuAn(string connection)
        {
            string select_proc = "HienThiDanhSachDuAn";
            using (SqlConnection conn = new SqlConnection(connection))
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = select_proc;
                    cmd.CommandType = CommandType.StoredProcedure;
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                Console.WriteLine("{0}\t{1}\t{2}", reader["sMaDA"],
                                    reader["sTenDA"],
                                    reader["sDiaDiem"]);
                            }
                        }
                    }

                }
            }
        }


        private static void chinhSuaDanhSachDuAn(string connection, string maDa, string tenDa, string diaDiem)
        {
            string update_proc = "Update_Du_an";
            using(SqlConnection conn = new SqlConnection(connection))
            {
                using(SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = update_proc;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@sMaDa", maDa);
                    cmd.Parameters.AddWithValue("@sTenDa", tenDa);
                    cmd.Parameters.AddWithValue("@sDiaDiem", diaDiem);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
            }
        }

        private static void xoaDuAnTheoMaDuAn(string connection, string maDa)
        {
            string delete_proc = "DELETE_DU_AN";
            using(SqlConnection conn = new SqlConnection(connection))
            {
                using(SqlCommand cmd=conn.CreateCommand())
                {
                    cmd.CommandText = delete_proc;
                    cmd.CommandType= CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@sMaDa", maDa);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
            }
        }

        private static void hienThiDanhSachDuAnNgatKetNoi(string connectionString)
        {
            string select_pro = "HienThiDanhSachDuAn";
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                //conn.ConnectionString = connectionString;
                using (SqlCommand cmd = conn.CreateCommand()) {
                    cmd.CommandText = select_pro;
                    cmd.CommandType= CommandType.StoredProcedure;
                    using (SqlDataAdapter adapter = new SqlDataAdapter())
                    {
                        adapter.SelectCommand = cmd;
                        using(DataTable dataTable = new DataTable())
                        {
                            adapter.Fill(dataTable);
                            if(dataTable.Rows
                                .Count > 0)
                            {
                                // hien ra man hinh du dung dataview
                                using(DataView dataView = new DataView(dataTable))
                                {
                                    // LỌC THEO ĐIỀU  KIỆN
                                    dataView.RowFilter = " sDiaDiem = N'HÀ NỘI' ";
                                }

                                // hien thi ra man hinh truc tiep
                                //foreach(DataRow row in dataTable.Rows)
                                //{
                                //    Console.WriteLine("{0}\t{1}\t{2}", row["sMaDA"], row["sTenDA"], row["sDiaDiem"]);
                                //}
                            }
                            else
                            {
                                // khong co ban ghi nao ton tai
                            }
                        }
                    }
                }
            }
        }

        private static bool xoaDuAnNgatKetNoi (string connectionString, string sMaDA)
        {
            string delete_proc = "DELETE_DU_AN";
            string select_pro = "HienThiDanhSachDuAn";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using(SqlCommand cmd = connection.CreateCommand())
                {
                    cmd.CommandText= select_pro;
                    cmd.CommandType = CommandType.StoredProcedure;
                    using(SqlDataAdapter adapter = new SqlDataAdapter())
                    {
                        adapter.SelectCommand = cmd;
                        using(DataTable dataTableDu_An = new DataTable("DU_AN"))
                        {
                            adapter.Fill (dataTableDu_An);
                            using(DataSet dataSet = new DataSet())
                            {
                                // add tung dataTable vao DataSet
                                dataSet.Tables.Add(dataTableDu_An);

                                // tim den maDa can xoa
                                dataTableDu_An.PrimaryKey = new DataColumn[] { dataTableDu_An.Columns["sMaDA"]};
                                DataRow dataRow = dataTableDu_An.Rows.Find(sMaDA);
                                dataRow.Delete();

                                // dong bo du lieu sql server thong qua thuoc tinh delete command cua data adaper
                                cmd.CommandText = delete_proc;
                                cmd.CommandType = CommandType.StoredProcedure;
                                // neu ben trn dung cmd.paramater ma da truyen tham so thi ben 
                                // duoi muon truyen tiep tham so thi phai clear no di
                                cmd.Parameters.Clear();
                                cmd.Parameters.AddWithValue("@sMaDa", sMaDA);

                                adapter.DeleteCommand = cmd;
                                int i = adapter.Update(dataSet, "DU_AN");
                                return i > 0;
                            }
                        }
                    }
                }
            }
        }

        private static bool themDuAnNgatKetNoi(string connectionString, string maDa, string tenDa, string diaDiem)
        {
            string insert_proc = "";
            using(SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlDataAdapter adapter = new SqlDataAdapter())
                {
                    adapter.SelectCommand = new SqlCommand("SELECT * FROM DU_AN", conn);
                    DataTable dataTableDu_An = new DataTable("DU_AN");
                    adapter.Fill(dataTableDu_An);

                    // add tung dataTable vao dataset
                    DataSet dataSet = new DataSet();
                    dataSet.Tables.Add(dataTableDu_An);

                    // them 1 dong moi vao table
                    DataRow row = dataTableDu_An.NewRow();
                    row["sMaDA"] = maDa;
                    row["sTenDA"] = tenDa;
                    row["sDiaDiem"] = diaDiem;

                    // dong bo du lieu vao CSDL
                    using(SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = insert_proc;
                        cmd.Parameters.AddWithValue("@sMaDa", maDa);
                        // parameter ....

                        adapter.InsertCommand = cmd;
                        int i = adapter.Update(dataSet , "DU_AN");
                        return i > 0;
                    }

                    // PARAMATER

                    

                }
            }
        }

    }
}
