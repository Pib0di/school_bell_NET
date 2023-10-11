using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using ExcelDataReader;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Media;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

/*
	Если кто-то читает этот код, то я ему сочувствую. Написан он был на коленке
 во время дедлайна, но все же...
*/


namespace school_bell_NET
{
	public partial class Form1 : Form
	{
		private string file_name = string.Empty;
		private SoundPlayer player;
		DataTable table;
        DateTime last_bell;

        private DataTableCollection tableCollection = null;

		public Form1()
		{
			InitializeComponent();
        }

        //воспроизвести звуковой сигнал
        private void button1_Click(object sender, EventArgs e)
        {
            picBell.Visible = false;
            if (player != null)
            {
                player.Play();
            }
        }

        //обновить данные таблицы
        private void btnClickThis_Click(object sender, EventArgs e)
		{
            if (toolStripComboBox1.Items.Count != 0)
            {
                table = tableCollection[Convert.ToString(toolStripComboBox1.SelectedItem)];

                dataGridView1.Rows.Clear();
                dataGridView1.Rows.Add(12);

                for (int i = 0; i < 5; ++i)
                {
                    dataGridView1.Columns[i].DefaultCellStyle.Format = i > 0 ? "HH:mm" : null;
                    for (int j = 0; j < table.Rows.Count; ++j)
                    {
                        if (table.AsDataView().Table.Rows[j][i] != null)
                        {
                            dataGridView1.Rows[j].Cells[i].Value = table.AsDataView().Table.Rows[j][i];
                        }
                        else
                        {
                            dataGridView1.Rows[j].Cells[i].Value = null;
                        }
                    }
                }
            }
        }

        private void update()
        {
            dataGridView1.Rows.Clear();
            dataGridView1.Rows.Add(12);

            for (int i = 0; i < 5; ++i)
            {
                dataGridView1.Columns[i].DefaultCellStyle.Format = i > 0 ? "HH:mm" : null;
                for (int j = 0; j < table.Rows.Count; ++j)
                {
                    if (table.AsDataView().Table.Rows[j][i] != null)
                    {
                        dataGridView1.Rows[j].Cells[i].Value = table.AsDataView().Table.Rows[j][i];
                    }
                    else
                    {
                        dataGridView1.Rows[j].Cells[i].Value = null;
                    }
                }
            }
        }

		private void OpenExcelFile(string path)
        {
			FileStream stream = File.Open(path, FileMode.Open, FileAccess.ReadWrite);
            IExcelDataReader reader = ExcelReaderFactory.CreateReader(stream);
           
            DataSet db = reader.AsDataSet(new ExcelDataSetConfiguration()
			{
				ConfigureDataTable = (x) => new ExcelDataTableConfiguration()
				{
					UseHeaderRow = true
				}
			});

			tableCollection = db.Tables;

			toolStripComboBox1.Items.Clear();
			foreach (DataTable table in tableCollection)
            {
				toolStripComboBox1.Items.Add(table.TableName);
            }

			toolStripComboBox1.SelectedIndex = 0;
		}

        //открытие excel файла
        private void открытьToolStripMenuItem_Click(object sender, EventArgs e)
		{
			try
			{
				DialogResult res = openFileDialog1.ShowDialog();

				if (res == DialogResult.OK)
				{
					file_name = openFileDialog1.FileName;

					Text = file_name;

					OpenExcelFile(file_name);
				}
				else
				{
					throw new Exception("Файл не выбран!");
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			
		}

        //выбор пресета для звонка из toolStrip...
		private void toolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e)
		{
            table = tableCollection[Convert.ToString(toolStripComboBox1.SelectedItem)];
            dataGridView1.Rows.Clear();
            dataGridView1.Rows.Add(12);
            for (int i = 0; i < 5; ++i)
            {
                dataGridView1.Columns[i].DefaultCellStyle.Format = i > 0 ? "HH:mm" : null;
                for (int j = 0; j < table.Rows.Count; ++j)
                {
                    if (table.AsDataView().Table.Rows[j][i] != null)
                    {
                        dataGridView1.Rows[j].Cells[i].Value = table.AsDataView().Table.Rows[j][i];
                    }
                    else
                    {
                        dataGridView1.Rows[j].Cells[i].Value = null;
                    }
                }
            }
        }

		private void Form1_Load(object sender, EventArgs e)
		{
            timer1.Start();
			timer2.Start();
        }

		private void timer1_Tick(object sender, EventArgs e)
		{
            //вывод времени
            System.DateTime time = System.DateTime.Now;
			if (time.Hour < 10)
            {
				label_time.Text = '0' + time.Hour.ToString() + ':';
            }
			else
            {
				label_time.Text = time.Hour.ToString() + ':';
			}

			if (time.Minute < 10)
			{
				label_time.Text += '0' + time.Minute.ToString() + ':';
			}
            else
            {
				label_time.Text += time.Minute.ToString() + ':';
            }

			if (time.Second < 10)
			{
				label_time.Text += '0' + time.Second.ToString() + ':';
			}
            else
            {
				label_time.Text += time.Second.ToString();
            }
                
			//Следующий звонок через
			hh.Text = Math.Abs(last_bell.Hour - time.Hour).ToString();
			mm.Text = Math.Abs(last_bell.Minute - time.Minute).ToString();
			ss.Text = Math.Abs(60 - time.Second).ToString();

            //Выбор пресета рассписания в (00:00)
            if (
                toolStripComboBox1.Items.Count != 0 &&
                time.Minute == 0 && 
                time.Hour == 0)
			{
                string dayToday = DateTime.Now.DayOfWeek.ToString();
				string selected_day = toolStripComboBox1.SelectedItem.ToString();
                /*if (dayToday == "Saturday" || dayToday == "Sunday" ||
                    dayToday == "Wednesday" || dayToday == "Friday" ||
                    dayToday == "Monday" || dayToday == "Tuesday" ||
                    dayToday == "Thursday")
                {*/
                //dayToday = "Sunday";
                    if (dayToday == "Sunday")//вс
                    {
                        dataGridView1.Rows.Clear();
                    }
                    else if (dayToday == "Saturday")//сб
					{
						table = tableCollection["Saturday"];
                        update();
                    }
                    else if (dayToday == "Wednesday")//ср
					{
						table = tableCollection["Wednesday"];
                        update();
                    }
                    else if (dayToday == "Monday")//пн
                    {
                        table = tableCollection["Monday"];
                        update();
                    }
                    else
					{
                        table = tableCollection["everyday"];
                        update();
                    }
/*                }
                else
                {
                    table = tableCollection[Convert.ToString(toolStripComboBox1.SelectedItem)];
                    update();
                }*/
            }
        }

        //Воспроизведение звонка 
		private void timer2_Tick(object sender, EventArgs e)
		{
            System.DateTime time = System.DateTime.Now;
            DateTime timeBell;
            for (int i = 0; i < dataGridView1.Rows.Count - 1; i++)
            {
                for (int j = 1; j != 3; ++j)
                {
                    if (dataGridView1.Rows[i].Cells[j].Value == null)
                    {
                        continue;
                    }

                    timeBell = Convert.ToDateTime(dataGridView1.Rows[i].Cells[j].Value);

					if (timeBell.Hour == time.Hour && timeBell.Minute == time.Minute )
                    {
                        picBell.Visible = true;

                        //а вот и он
                        if (player != null && timeBell.Hour != 0)
                        {
                            player.Play();
                        }
      //                  /*if (j == 1)
						//{
      //                      if (dataGridView1.Rows[i].Cells[j + 1].Value != null)
      //                      {
      //                          last_bell = Convert.ToDateTime(dataGridView1.Rows[i].Cells[j + 1].Value);
      //                      }
      //                  }
						//else
						//{
      //                      if (dataGridView1.Rows[i + 1].Cells[j - 1].Value != null)
      //                      {
      //                          last_bell = Convert.ToDateTime(dataGridView1.Rows[i + 1].Cells[j - 1].Value);
      //                      }
      //                  }*/
                    }
                }
            }
        }

        //открытие звикового файла (.wav)
        private void звонокToolStripMenuItem_Click_1(object sender, EventArgs e)
		{
            try
            {
                DialogResult res = openFileDialog2.ShowDialog();

                if (res == DialogResult.OK)
                {
                    file_name = openFileDialog2.FileName;
                    Text = file_name;

                    player = new SoundPlayer(file_name);
                }
                else
                {
                    throw new Exception("Файл не выбран!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void файлToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
		{

		}

        private void toolStripComboBox1_Click(object sender, EventArgs e)
        {

        }
        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void monthCalendar1_DateChanged(object sender, DateRangeEventArgs e)
        {

        }
    }
}
