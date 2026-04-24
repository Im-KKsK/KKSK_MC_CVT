using LevelDB;
using MaterialSkin;
using MaterialSkin.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.IO.Compression;

using System.Linq;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Resources.ResXFileRef;
using static System.Windows.Forms.Design.AxImporter;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;


namespace WindowsFormsApp6
{
    
    public partial class Form1 : Form
    {

        [DllImport("XOREncryptDLL.dll", EntryPoint = "check_file_is_encrypt", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
        static extern int FileEncryptionCheck(IntPtr Src);

        [DllImport("XOREncryptDLL.dll", EntryPoint = "decrypt_file", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
        static extern int DecryptFile(IntPtr Src, int SrcLength, out IntPtr Buff, out int Length);
        string IMPORT_PATH = null;
        string EXPORT_PATH = null;

        public Form1()
        {

            InitializeComponent();
            webBrowser1.Url = new Uri(Path.Combine(Application.StartupPath, "page.html"));
        }

        private void tabPage1_Click(object sender, EventArgs e)
        {

        }

        private void materialButton1_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                IMPORT_PATH = folderBrowserDialog1.SelectedPath;
                materialTextBox21.Text = IMPORT_PATH;

            }
        }

        private void materialButton2_Click(object sender, EventArgs e)
        {
            
            if (folderBrowserDialog2.ShowDialog() == DialogResult.OK)
            {
                EXPORT_PATH = folderBrowserDialog2.SelectedPath + "\\KKSK_CVT_WORLD";
                materialTextBox22.Text = EXPORT_PATH;

            }

            //}
        }

        private void materialButton3_Click(object sender, EventArgs e)
        {
            materialMultiLineTextBox21.Text = null;
            materialProgressBar1.Value = 0;
            materialProgressBar1.Maximum = 100;
            materialProgressBar1.Minimum = 0;


            if (string.IsNullOrEmpty(IMPORT_PATH) || string.IsNullOrEmpty(EXPORT_PATH))
            {
                materialMultiLineTextBox21.Text = materialMultiLineTextBox21.Text + ("请先选择导入和导出路径！\r\n");
                materialProgressBar1.Value = 100;
                label18.Text = "当前操作：操作失败。";
                MessageBox.Show("请先选择导入和导出路径！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if(materialComboBox1.SelectedIndex == 1 && string.IsNullOrEmpty(saveFileDialog1.FileName))
            {
                materialMultiLineTextBox21.Text = materialMultiLineTextBox21.Text + ("请先选择《我的世界》存档文件保存路径！\r\n");
                materialProgressBar1.Value = 100;
                label18.Text = "当前操作：操作失败。";
                MessageBox.Show("请先选择《我的世界》存档文件保存路径！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            
            try
            {

                materialMultiLineTextBox21.Text = materialMultiLineTextBox21.Text + ($"正在复制存档从 {IMPORT_PATH} 到 {EXPORT_PATH}...\r\n");
                label18.Text = "当前操作：正在复制文件。";

                if (Directory.Exists(EXPORT_PATH))
                {
                    Directory.Delete(EXPORT_PATH, true);
                }


                CopyDirectory(IMPORT_PATH, EXPORT_PATH, true);
                materialMultiLineTextBox21.Text = materialMultiLineTextBox21.Text + ("存档复制完成！\r\n");
                label18.Text = "当前操作：复制成功。";
                materialProgressBar1.Value = 5;


                DirectoryInfo info = new DirectoryInfo(EXPORT_PATH);
                List<FileInfo> files = new List<FileInfo>();


                foreach (DirectoryInfo dirinfo in info.GetDirectories())
                {
                    foreach (FileInfo file in dirinfo.GetFiles())
                        files.Add(file);
                }
                foreach (FileInfo file in info.GetFiles())
                    files.Add(file);

                materialMultiLineTextBox21.Text = materialMultiLineTextBox21.Text + ("开始检查目标存档文件夹并生成需要转换的文件列表：\r\n");
                label18.Text = "当前操作：正在检查需要转换的文件。";
                materialProgressBar1.Value = 8;
                List<FileInfo> toDecrypt = new List<FileInfo>();


                foreach (FileInfo file in files)
                {
                    byte[] targetFilePath = Encoding.UTF8.GetBytes(file.FullName);
                    GCHandle handle = GCHandle.Alloc(targetFilePath, GCHandleType.Pinned);
                    IntPtr intPtr = handle.AddrOfPinnedObject();
                    if (FileEncryptionCheck(intPtr) == 1)
                    {
                        toDecrypt.Add(file);
                        materialMultiLineTextBox21.Text = materialMultiLineTextBox21.Text + ($"文件 {file.Name} 需要转换\r\n");
                        label18.Text = "当前操作：正在寻找需要转换的文件。";
                    }
                    handle.Free();
                }

                materialMultiLineTextBox21.Text = materialMultiLineTextBox21.Text + ($"发现 {toDecrypt.Count} 个需要转换的文件\r\n");
                label18.Text = $"当前操作：发现{toDecrypt.Count} 个需要转换的文件，开始转换。";
                materialMultiLineTextBox21.Text = materialMultiLineTextBox21.Text + ("开始转换文件：\r\n");


                foreach (FileInfo file in toDecrypt)
                {
                    byte[] targetFilePath = Encoding.UTF8.GetBytes(file.FullName);
                    GCHandle handle = GCHandle.Alloc(targetFilePath, GCHandleType.Pinned);
                    IntPtr intPtr = handle.AddrOfPinnedObject();
                    IntPtr resultPathPtr = IntPtr.Zero;
                    int Length = 0;

                    if (DecryptFile(intPtr, file.FullName.Length, out resultPathPtr, out Length) != 0)
                    {
                        materialMultiLineTextBox21.Text = materialMultiLineTextBox21.Text + ($"{file.Name} 转换失败\r\n");
                        handle.Free();
                        return;
                    }

                    byte[] resultPath = new byte[Length];
                    Marshal.Copy(resultPathPtr, resultPath, 0, Length);
                    handle.Free();
                    Marshal.FreeHGlobal(resultPathPtr);


                    File.Copy(Encoding.UTF8.GetString(resultPath), file.FullName, true);
                    File.Delete(Encoding.UTF8.GetString(resultPath));

                    materialMultiLineTextBox21.Text = materialMultiLineTextBox21.Text + ($"{file.Name} 转换成功\r\n");
                    materialProgressBar1.Value = materialProgressBar1.Value + (50 / (int)toDecrypt.Count);
                }

                materialMultiLineTextBox21.Text = materialMultiLineTextBox21.Text + ("存档转换已完全成功！\r\n");
                label18.Text = "当前操作：文件转换成功。";
                materialMultiLineTextBox21.Text = materialMultiLineTextBox21.Text + ($"转换后的存档保存在：{EXPORT_PATH}\r\n");
                materialProgressBar1.Value = 70;

            }
            catch (Exception ex)
            {
                materialProgressBar1.Value = 100;
                materialMultiLineTextBox21.Text = materialMultiLineTextBox21.Text + ($"发生错误：{ex.Message}\r\n");
                
                label18.Text = "当前操作：转换文件失败。";

                MessageBox.Show($"转换失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

                if (materialCheckbox1.Checked == false)
            {
                materialProgressBar1.Value = 100;
                MessageBox.Show("存档转换成功！");
                materialMultiLineTextBox21.Text = materialMultiLineTextBox21.Text + ("存档转换成功！感谢您使用此项目。\r\n");
                materialMultiLineTextBox21.Text = materialMultiLineTextBox21.Text + ("如果帮助到您，您可以考虑前往 Github 发布页点亮项目 Star 。软件永久免费且开源。\r\n");

                materialMultiLineTextBox21.Text = materialMultiLineTextBox21.Text + ("您可以点击右上方清空输出栏的所有内容。\r\n");

            }
                if (materialCheckbox1.Checked == true)
            {
                materialMultiLineTextBox21.Text = materialMultiLineTextBox21.Text + ("开始执行人物移速、视角修复\r\n");
                label18.Text = "当前操作：文件转换成功，开始执行移速、视角修复。";

                try
                {

                    string dbPath = Path.Combine(EXPORT_PATH, "db");

                    if (!Directory.Exists(dbPath))
                    {
                        materialMultiLineTextBox21.Text = materialMultiLineTextBox21.Text + ($"未找到 db 文件夹：{dbPath}\r\n");
                        materialMultiLineTextBox21.Text = materialMultiLineTextBox21.Text + ("修复执行失败\r\n");
                        label18.Text = "当前操作：存档转换已完成，但修复失败。";

                        materialProgressBar1.Value = 100;
                        MessageBox.Show($"未找到 db 文件夹，移速修复失败。：{dbPath}", "错误",

                        MessageBoxButtons.OK, MessageBoxIcon.Error);

                        return;
                    }


                    ClearPlayerDataFromLevelDB(dbPath);

                }
                catch (Exception ex)
                {
                    materialMultiLineTextBox21.Text = materialMultiLineTextBox21.Text + ($"清理玩家数据时出错：{ex.Message}\r\n");
                    materialMultiLineTextBox21.Text = materialMultiLineTextBox21.Text + ("移速修复执行失败\r\n");
                    materialProgressBar1.Value = 100;
                    label18.Text = "当前操作：存档转换已完成，但修复失败。";

                    MessageBox.Show($"清理玩家数据时出错，移速修复失败。：{ex.Message}",
                        "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);

                }
            }
            
        }


        private void ClearPlayerDataFromLevelDB(string dbPath)
        {
            bool deleted = false;
            try
            {
                var options = new LevelDB.Options { CreateIfMissing = false };

                using (var db = new LevelDB.DB(options, dbPath))
                {

                    deleted = DeleteLocalPlayerKey(db);
                }
                if (deleted)
                {
                    materialMultiLineTextBox21.Text = materialMultiLineTextBox21.Text + ("已修复异常项目！\r\n");
                    if (materialComboBox1.SelectedIndex == 0)
                    {
                        materialProgressBar1.Value = 100;

                        label18.Text = "当前操作：操作完全成功！";

                        materialMultiLineTextBox21.Text = materialMultiLineTextBox21.Text + ("恭喜！您已经成功转换并修复了您的存档！祝您玩得愉快！\r\n");
                        materialMultiLineTextBox21.Text = materialMultiLineTextBox21.Text + ("如果帮助到您，您可以考虑前往 Github 发布页点亮项目 Star 。软件永久免费且开源。\r\n");
                        materialMultiLineTextBox21.Text = materialMultiLineTextBox21.Text + ("您可以点击右上方清空输出栏的所有内容。\r\n");
                        MessageBox.Show("恭喜！您已经顺利完成了转换，并自动修复了移速和视角问题。祝您玩得愉快！\r\n",
                            "操作成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }


                    if (materialComboBox1.SelectedIndex == 1)
                    {


                        try
                        {
                            materialProgressBar1.Value = 95;
                            label18.Text = "当前操作：打包为《我的世界》存档文件";
                            materialMultiLineTextBox21.Text = materialMultiLineTextBox21.Text + ("正在打包为《我的世界》存档文件...\r\n");

                            ZipFile.CreateFromDirectory(EXPORT_PATH, saveFileDialog1.FileName, System.IO.Compression.CompressionLevel.NoCompression, false);
                            materialMultiLineTextBox21.Text = materialMultiLineTextBox21.Text + ("打包成功！\r\n");
                            materialProgressBar1.Value = 100;

                            label18.Text = "当前操作：操作完全成功！";

                            materialMultiLineTextBox21.Text = materialMultiLineTextBox21.Text + ("恭喜！您已经成功转换并修复了您的存档！祝您玩得愉快！\r\n");
                            materialMultiLineTextBox21.Text = materialMultiLineTextBox21.Text + ("如果帮助到您，您可以考虑前往 Github 发布页点亮项目 Star 。软件永久免费且开源。\r\n");
                            materialMultiLineTextBox21.Text = materialMultiLineTextBox21.Text + ("您可以点击右上方清空输出栏的所有内容。\r\n");
                            MessageBox.Show("恭喜！您已经顺利完成了转换，并自动修复了移速和视角问题。祝您玩得愉快！\r\n",
                                "操作成功", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        }
                        catch (Exception ex)
                        {
                            materialMultiLineTextBox21.Text = materialMultiLineTextBox21.Text + ("打包为《我的世界》存档文件失败\r\n");
                            
                            materialProgressBar1.Value = 100;

                            label18.Text = "当前操作：存档转换成功且成功执行修复，但导出为《我的世界》存档文件时失败。";

                            materialMultiLineTextBox21.Text = materialMultiLineTextBox21.Text + "导出为《我的世界》存档文件时失败！\r\n" +
                            "可能的原因：\r\n" +
                            "1.该文件夹中已存在同名文件。r\n" +
                            "2.没有读写权限。\r\n";

                            materialMultiLineTextBox21.Text = materialMultiLineTextBox21.Text + "虽然《我的世界》存档文件生成失败，但您可以仍然直接复制整个存档文件夹使用\r\n";
                            MessageBox.Show("导出为《我的世界》存档文件时失败！\r\n" +
                            "可能的原因：\r\n" +
                            "1.该文件夹中已存在同名文件。\r\n" +
                            "2.没有读写权限。\r\n" + "虽然《我的世界》存档文件生成失败，但您可以仍然直接复制整个存档文件夹使用\r\n",
                            "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                        }



                    }
                    



                }
                else
                {
                    materialMultiLineTextBox21.Text = materialMultiLineTextBox21.Text + "未找到需要修复的内容！\r\n" +
                        "可能的原因：\r\n" +
                        "1.该存档中数据丢失或损坏\r\n" +
                        "2.已经执行过修复或无需修复。\r\n";
                    materialProgressBar1.Value = 100;
                    label18.Text = "当前操作：存档转换成功，但未执行修复，因为没有找到需要修复的项目。";

                    MessageBox.Show("未找到需要修复的内容！\r\n" +
                        "可能的原因：\r\n" +
                        "1.该存档中数据丢失或损坏\r\n" +
                        "2.已经执行过修复或无需修复。\r\n",
                        "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                materialProgressBar1.Value = 100;
                label18.Text = "当前操作：未知错误。";
                throw new Exception($"处理数据库时出错：{ex.Message}", ex);

            }

        }


        
        private bool DeleteLocalPlayerKey(LevelDB.DB db)
        {
            string keyToDelete = "~local_player";
            bool deleted = false;


            try
            {
                byte[] keyBytes = Encoding.UTF8.GetBytes(keyToDelete);
                byte[] value = db.Get(keyBytes);

                if (value != null)
                {
                    label18.Text = "当前操作：执行修复。";

                    materialMultiLineTextBox21.Text = materialMultiLineTextBox21.Text + ("已发现需要修复的项目！开始执行修复。\r\n");
                    materialProgressBar1.Value = 90;
                    db.Delete(keyBytes);
                    deleted = true;
                }
            }
            catch
            {

                try
                {
                    byte[] keyBytes = Encoding.ASCII.GetBytes(keyToDelete);
                    byte[] value = db.Get(keyBytes);

                    if (value != null)
                    {
                        label18.Text = "当前操作：执行修复。";
                        materialMultiLineTextBox21.Text = materialMultiLineTextBox21.Text + ("已发现需要修复的项目！开始执行修复。\r\n");
                        materialProgressBar1.Value = 90;
                        db.Delete(keyBytes);
                        deleted = true;
                    }
                }
                catch
                {

                }
            }

            return deleted;
        }

        private void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
        {
            var dir = new DirectoryInfo(sourceDir);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException($"源目录不存在: {dir.FullName}");

            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            Directory.CreateDirectory(destinationDir);


            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                file.CopyTo(targetFilePath, true);
            }


            if (recursive)
            {
                foreach (DirectoryInfo subDir in dirs)
                {
                    string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                    CopyDirectory(subDir.FullName, newDestinationDir, true);
                }
            }
        }

        private void materialComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

            if (materialComboBox1.SelectedIndex == 0)
            {
          
                materialTextBox23.Enabled = false;
                materialButton5.Enabled = false;
                materialCheckbox1.Enabled = true;
                materialTextBox23.ReadOnly = true;
            }
            if (materialComboBox1.SelectedIndex == 1)

            {
               

                materialCheckbox1.Checked = true;
                materialCheckbox1.Enabled = false;
                materialTextBox23.Enabled = true;
                materialButton5.Enabled = true;
                materialTextBox23.ReadOnly = true;
            }
            EXPORT_PATH = null;
            materialTextBox22.Text = "（导出目录）等待用户输入";
        }

        private void materialButton4_Click(object sender, EventArgs e)
        {
            materialMultiLineTextBox21.Text = null;
        }

        private void tabPage2_Click(object sender, EventArgs e)
        {

        }

        private void materialButton5_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {

                materialTextBox23.Text = saveFileDialog1.FileName;
            }
        }
        // 十分感谢以下项目为本项目做出的贡献！
        private void materialButton6_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/IgnaceMaes/MaterialSkin");
        }

        private void materialButton7_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/KiwiR1der/MaterialSkin");
        }

        private void materialButton10_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/google/leveldb");

        }

        private void materialButton9_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.nuget.org/packages/LevelDB.Standard/");
        }

        private void materialButton11_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/Carbonateds/MCWorld-Converter");

        }

        private void materialButton8_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://developer.huawei.com/consumer/cn/design/resource-V1/");

        }
    }
    public class FontLoader
    {
        [DllImport("gdi32.dll")]
        private static extern int AddFontResource(string lpszFilename);

        [DllImport("gdi32.dll")]
        private static extern int RemoveFontResource(string lpFileName);

        [DllImport("user32.dll")]
        private static extern int SendMessageTimeout(int hWnd, uint Msg, int wParam, int lParam, uint fuFlags, uint uTimeout, out int lpdwResult);

        private static readonly uint WM_FONTCHANGE = 0x001D;
        private static readonly uint SMTO_ABORTIFHUNG = 0x0002;

        private static List<string> _loadedFonts = new List<string>();

        
        public static void LoadFonts()
        {
            try
            {
                string[] fontFiles = new string[]
                {
                "HarmonyOS_Sans_SC_Bold.ttf",
                "HarmonyOS_Sans_SC_Black.ttf",
                "HarmonyOS_Sans_SC_Light.ttf",
                "HarmonyOS_Sans_SC_Medium.ttf",
                "HarmonyOS_Sans_SC_Regular.ttf",
                "HarmonyOS_Sans_SC_Thin.ttf"
                };

                string appPath = Application.StartupPath;

                foreach (string fontFile in fontFiles)
                {
                    string fontPath = Path.Combine(appPath, fontFile);

                    if (File.Exists(fontPath))
                    {
                        int result = AddFontResource(fontPath);
                        if (result > 0)
                        {
                            _loadedFonts.Add(fontPath);
                            
                        }
                        else
                        {
                            //Console.WriteLine($"字体加载失败: {fontFile}");
                        }
                    }
                    else
                    {
                       // Console.WriteLine($"字体文件不存在: {fontFile}");
                    }
                }

                
                int dwResult;
                SendMessageTimeout(0xFFFF, WM_FONTCHANGE, 0, 0, SMTO_ABORTIFHUNG, 1000, out dwResult);

                Console.WriteLine("字体加载完成，已通知系统更新");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"加载字体时出错: {ex.Message}");
            }
        }

        
        public static void UnloadFonts()
        {
            try
            {
                foreach (string fontPath in _loadedFonts)
                {
                    RemoveFontResource(fontPath);
                }

                _loadedFonts.Clear();

                
                int dwResult;
                SendMessageTimeout(0xFFFF, WM_FONTCHANGE, 0, 0, SMTO_ABORTIFHUNG, 1000, out dwResult);

                
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"卸载字体时出错: {ex.Message}");
            }
        }

       
        public static PrivateFontCollection LoadFontsToPrivateCollection()
        {
            PrivateFontCollection privateFonts = new PrivateFontCollection();

            try
            {
                string[] fontFiles = new string[]
                {
                "HarmonyOS_Sans_SC_Bold.ttf",
                "HarmonyOS_Sans_SC_Black.ttf",
                "HarmonyOS_Sans_SC_Light.ttf",
                "HarmonyOS_Sans_SC_Medium.ttf",
                "HarmonyOS_Sans_SC_Regular.ttf",
                "HarmonyOS_Sans_SC_Thin.ttf"
                };

                string appPath = Application.StartupPath;

                foreach (string fontFile in fontFiles)
                {
                    string fontPath = Path.Combine(appPath, fontFile);

                    if (File.Exists(fontPath))
                    {
                        try
                        {
                            privateFonts.AddFontFile(fontPath);
                            
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("字体加载失败，您仍可以正常使用功能，但无法获得完整的视觉效果。您可以手动安装 HarmonyOS_Sans 系列字体。");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("字体加载失败，您仍可以正常使用功能，但无法获得完整的视觉效果。您可以手动安装 HarmonyOS_Sans 系列字体。");
            }

            return privateFonts;
        }
    }
}