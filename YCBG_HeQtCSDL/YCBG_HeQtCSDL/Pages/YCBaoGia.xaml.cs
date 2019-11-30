﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using YCBG_HeQtCSDL.ViewModel;

namespace YCBG_HeQtCSDL.Pages
{
    /// <summary>
    /// Interaction logic for YCBaoGia.xaml
    /// </summary>
    public partial class YCBaoGia : Page
    {
        YeuCauBaoGiaVM yeuCauBaoGiaVM;
        List<YeuCauBaoGiaVM> yeuCauBaoGiaVMs;
        string connectionString;
        public YCBaoGia(string connectionString)
        {
            InitializeComponent();
            this.connectionString = connectionString;
            get_YCBG();
        }

        private void Row_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            yeuCauBaoGiaVM = (YeuCauBaoGiaVM)dtgYCBG.SelectedItem;
            list_CTYCBG list_CTYCBG = new list_CTYCBG(connectionString, this.yeuCauBaoGiaVM);
            //chiTietBaoGiaVM = new ChiTietBaoGiaVM(((ChiTietBaoGiaVM)chiTietBaoGiaVMTemp));
            //MessageBox.Show(yeuCauBaoGiaVM.MaYCBG);
            list_CTYCBG.ShowDialog();
            // Some operations with this row

            //refresh when windows closed
            if (list_CTYCBG.isClosed)
            {
                get_YCBG();
                dtgYCBG.Items.Refresh();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ThemSanPhamYCBG themSanPhamYCBG = new ThemSanPhamYCBG(this.connectionString);
            themSanPhamYCBG.ShowDialog();

            //refresh when windows closed
            if (themSanPhamYCBG.isClosed)
            {
                get_YCBG();
                dtgYCBG.Items.Refresh();
            }
        }

        private void get_YCBG(string maYCBG = "")
        {

            yeuCauBaoGiaVMs = new List<YeuCauBaoGiaVM>();
            SqlDataReader rdr = null;

            using (var conn = new SqlConnection(this.connectionString))
            using (var command = new SqlCommand("sp_get_ycbg", conn)
            {
                CommandType = CommandType.StoredProcedure
            })
            {
                try
                {
                    conn.Open();
                    command.Parameters.AddWithValue("@maYCBG", maYCBG);
                    rdr = command.ExecuteReader();

                    while (rdr.Read())
                    {
                        yeuCauBaoGiaVM = new YeuCauBaoGiaVM();
                        yeuCauBaoGiaVM.MaYCBG = rdr["MaYCBaoGia"].ToString();
                        yeuCauBaoGiaVM.NgayYCBG = rdr["NgayYCBaoGia"].ToString();
                        yeuCauBaoGiaVM.TinhTrang = rdr["TinhTrang"].ToString();
                        yeuCauBaoGiaVM.MaNV = rdr["MaNV"].ToString();
                        yeuCauBaoGiaVMs.Add(yeuCauBaoGiaVM);
                    }
                    dtgYCBG.ItemsSource = yeuCauBaoGiaVMs;
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }
                finally
                {
                    conn.Close();
                }

            }
        }

        private void txtMaSP_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                get_YCBG(txtMaSP.Text);
                dtgYCBG.Items.Refresh();
            }
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService navService = NavigationService.GetNavigationService(this);
            showOption pg = new showOption(connectionString);
            navService.Navigate(pg);
        }
    }
}
