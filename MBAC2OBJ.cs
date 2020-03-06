using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace MBAC2OBJ
{
    public partial class Form1 : Form
    {
        public string fileName = "";
        public Form1()
        {
            InitializeComponent();
            textBox2.Text = Environment.CurrentDirectory + "\\";
            textBox4.Text = Environment.CurrentDirectory + "\\MascotCapsule_Archaeology-master";
        }

        private void textBox5_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/minexew/MascotCapsule_Archaeology/archive/master.zip");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDlg = openFileDialog1;
            openFileDialog1.Filter = "Mascot Capsule Files(*.mbac)|*.mbac;";
            if (fileDlg.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = fileDlg.FileName;
                fileName = fileDlg.FileName;
            }
        }

        private void textBox1_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Link;
            else
                e.Effect = DragDropEffects.None;
        }

        private void textBox1_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = e.Data.GetData(DataFormats.FileDrop) as string[]; // get all files droppeds  
            if (files != null && files.Any())
            {
                //pathIMG.Text = files.First(); //select the first one
                if (CheckFile(files.First()))
                {
                    textBox1.Text = files.First();
                }
            }
        }

        public bool CheckFile(string path)
        {
            if (File.Exists(path))
            {
                string extension = Path.GetExtension(path);
                if(extension.ToLower() == ".mbac")
                {
                    return true;
                    //textBox1.Text = path;
                }
                else
                {
                    
                    MessageBox.Show("File : \"" + path + "\" not is .mbac!", "Error file!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
            else
            {
                MessageBox.Show("The system cannot find the file : \"" + path + "\"!", "Error path", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderDlg = folderBrowserDialog1;
            folderDlg.ShowNewFolderButton = true;
            DialogResult result = folderDlg.ShowDialog();
            if (result == DialogResult.OK)
            {
                textBox2.Text = folderDlg.SelectedPath + "\\";
                Environment.SpecialFolder root = folderDlg.RootFolder;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderDlg = folderBrowserDialog2;
            folderDlg.ShowNewFolderButton = true;
            DialogResult result = folderDlg.ShowDialog();
            if (result == DialogResult.OK)
            {
                textBox4.Text = folderDlg.SelectedPath + "\\";
                Environment.SpecialFolder root = folderDlg.RootFolder;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if(textBox1.Text == "" || textBox2.Text == "" || textBox4.Text == "")
            {
                MessageBox.Show("Аill in all fields!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                if (CheckFile(textBox1.Text)&& checkPath(textBox2.Text) && checkPath(textBox4.Text))
                {
                    if (textBox3.Text == "")
                        textBox3.Text = "name";
                    if (Directory.Exists(textBox4.Text + "tools"))
                    {
                        if (File.Exists(textBox4.Text + "tools\\mbac2obj.py"))
                        {
                            string docPath = textBox4.Text + "tools";
                            string[] lines = { "cd "+ textBox4.Text + "tools" , "python mbac2obj.py " + textBox1.Text +" "+ textBox2.Text + textBox3.Text+".obj", "del %0" };
                            //string command = "'cd ' + textBox4.Text+ 'BMF_Editor.exe'";// + "\\tools "+ "python mbac2obj.py " + textBox1.Text + textBox3.Text+".obj" +"pause";
                            //System.Diagnostics.Process.Start("cmd.exe",  command);
                            using (StreamWriter outputFile = new StreamWriter(Path.Combine(docPath, "run.bat")))
                            {
                                foreach (string line in lines)
                                    outputFile.WriteLine(line);
                            }
                            //MessageBox.Show(docPath + "run.bat", "info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            System.Diagnostics.Process.Start(docPath+"\\run.bat");
                        }
                        else
                        {
                            MessageBox.Show("Missing mbac2obj.py file. You can download Mascot_Capsule_Archeology!", "Error mbac2obj.py!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show("You can download Mascot_Capsule_Archeology!", "Error Mascot_Capsule!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            
        }

        public bool checkPath(string path)
        {
            if (!Directory.Exists(path)){
                MessageBox.Show("The system cannot find the path : \"" + path + "\"!", "Error path", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }else
            {
                return true;
            }
           
        }

        private void textBox6_Click(object sender, EventArgs e)
        {
            Process.Start("https://www.python.org/downloads/");
        }
    }
}
