using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace pry.Articulos_Tumminelli
{
    public partial class frmArticulos : Form
    {
        private List<Articulo> listaArticulos = new List<Articulo>();

        public frmArticulos()
        {
            InitializeComponent();
        }

        private string RutaArchivo(string nombreArchivo)
        {
            return Path.Combine(Application.StartupPath, nombreArchivo);
        }

        private void frmArticulos_Load(object sender, EventArgs e)
        {
            try
            {
                ConfigurarGrilla();
                CargarRubros();
                CargarArticulos();

                txtCantidadArticulos.Text = "0";
                txtTotal.Text = "$ 0,00";

                txtCantidadArticulos.ReadOnly = true;
                txtTotal.ReadOnly = true;
            }
            catch (Exception error)
            {
                MessageBox.Show(
                    "No se pudieron cargar los archivos CSV.\n\n" + error.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void ConfigurarGrilla()
        {
            dataGridView1.AutoGenerateColumns = false;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.ReadOnly = true;
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = false;

            dataGridView1.BackgroundColor = Color.White;
            dataGridView1.BorderStyle = BorderStyle.FixedSingle;

            dataGridView1.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.Fill;

            dataGridView1.ColumnHeadersDefaultCellStyle.Alignment =
                DataGridViewContentAlignment.MiddleCenter;

            dataGridView1.Columns.Clear();

            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Código",
                DataPropertyName = "Codigo",
                FillWeight = 15
            });

            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Descripción",
                DataPropertyName = "Descripcion",
                FillWeight = 40
            });

            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Costo",
                DataPropertyName = "Costo",
                FillWeight = 14,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = "C2",
                    FormatProvider = new CultureInfo("es-AR"),
                    Alignment = DataGridViewContentAlignment.MiddleRight
                }
            });

            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Stock",
                DataPropertyName = "Stock",
                FillWeight = 11,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleCenter
                }
            });

            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Valor en Stock",
                DataPropertyName = "ValorEnStock",
                FillWeight = 20,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = "C2",
                    FormatProvider = new CultureInfo("es-AR"),
                    Alignment = DataGridViewContentAlignment.MiddleRight
                }
            });
        }

        private void CargarRubros()
        {
            cmbRubros.Items.Clear();

            string[] rubros = File.ReadAllLines(RutaArchivo("RUBROS.csv"));

            foreach (string rubro in rubros)
            {
                cmbRubros.Items.Add(rubro.Trim());
            }

            cmbRubros.SelectedIndex = -1;
        }

        private void CargarArticulos()
        {
            listaArticulos.Clear();

            string[] lineas = File.ReadAllLines(RutaArchivo("ARTICULOS.csv"));

            foreach (string linea in lineas)
            {
                string[] datos = linea.Split(';');

                if (datos.Length == 5)
                {
                    Articulo articulo = new Articulo();

                    articulo.Codigo = datos[0];
                    articulo.Descripcion = datos[1];
                    articulo.Costo = decimal.Parse(datos[2]);
                    articulo.Rubro = datos[3];
                    articulo.Stock = int.Parse(datos[4]);

                    listaArticulos.Add(articulo);
                }
            }
        }

        private void btnMostrar_Click(object sender, EventArgs e)
        {
            if (cmbRubros.SelectedIndex == -1)
            {
                MessageBox.Show(
                    "Seleccione un rubro antes de continuar.",
                    "Atención",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            string rubroElegido = cmbRubros.Text;

            List<Articulo> articulosFiltrados = listaArticulos
                .Where(articulo => articulo.Rubro == rubroElegido)
                .ToList();

            dataGridView1.DataSource = null;
            dataGridView1.DataSource = articulosFiltrados;

            txtCantidadArticulos.Text = articulosFiltrados.Count.ToString();

            decimal total = articulosFiltrados.Sum(articulo => articulo.ValorEnStock);

            txtTotal.Text = total.ToString("C2", new CultureInfo("es-AR"));
        }

        private void btnExportar_Click(object sender, EventArgs e)
        {
            if (cmbRubros.SelectedIndex == -1)
            {
                MessageBox.Show(
                    "Seleccione un rubro antes de exportar.",
                    "Atención",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            string rubroElegido = cmbRubros.Text;

            List<Articulo> articulosFiltrados = listaArticulos
                .Where(articulo => articulo.Rubro == rubroElegido)
                .ToList();

            if (articulosFiltrados.Count == 0)
            {
                MessageBox.Show(
                    "No hay artículos para exportar.",
                    "Atención",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            using (SaveFileDialog guardarArchivo = new SaveFileDialog())
            {
                guardarArchivo.Filter = "Archivo CSV|*.csv";
                guardarArchivo.Title = "Exportar artículos";
                guardarArchivo.FileName = "Articulos_" + rubroElegido + ".csv";

                if (guardarArchivo.ShowDialog() == DialogResult.OK)
                {
                    List<string> lineas = new List<string>();

                    lineas.Add("Código;Descripción;Costo;Stock;Valor en Stock");

                    foreach (Articulo articulo in articulosFiltrados)
                    {
                        string linea =
                            articulo.Codigo + ";" +
                            articulo.Descripcion + ";" +
                            articulo.Costo.ToString("0.00", CultureInfo.InvariantCulture) + ";" +
                            articulo.Stock + ";" +
                            articulo.ValorEnStock.ToString("0.00", CultureInfo.InvariantCulture);

                        lineas.Add(linea);
                    }

                    File.WriteAllLines(
                        guardarArchivo.FileName,
                        lineas,
                        Encoding.UTF8);

                    MessageBox.Show(
                        "Archivo exportado correctamente.",
                        "Exportación finalizada",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
        }

        private void lnkAcercaDe_LinkClicked(
            object sender,
            LinkLabelLinkClickedEventArgs e)
        {
            using (frmAcercaDe ventana = new frmAcercaDe())
            {
                ventana.ShowDialog();
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {
        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {
        }

        private void lblrubro_Click(object sender, EventArgs e)
        {
        }

        private void dataGridView1_CellContentClick(
            object sender,
            DataGridViewCellEventArgs e)
        {
        }

        private void groupBox3_Enter(object sender, EventArgs e)
        {
        }

        private void cmbRubros_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void txtTotal_TextChanged(object sender, EventArgs e)
        {
        }

        private void txtCantidadArticulos_TextChanged(object sender, EventArgs e)
        {
        }
    }

    public class Articulo
    {
        public string Codigo { get; set; }
        public string Descripcion { get; set; }
        public decimal Costo { get; set; }
        public string Rubro { get; set; }
        public int Stock { get; set; }

        public decimal ValorEnStock
        {
            get
            {
                return Costo * Stock;
            }
        }
    }
}