using Microsoft.AspNetCore.Mvc;
using MigraDocCore.DocumentObjectModel;
using MigraDocCore.DocumentObjectModel.MigraDoc.DocumentObjectModel.Shapes;
using MigraDocCore.DocumentObjectModel.Shapes;
using MigraDocCore.DocumentObjectModel.Tables;
using MigraDocCore.Rendering;
using MikaWeb.Extensions.DB;
using MikaWeb.Models;
using MikaWeb.Models.ViewModels;
using PdfSharpCore.Utils;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;

namespace MikaWeb.Extensions
{
    public class ExportPDF
    {

        private string content;
        DBExtension db;

        public ExportPDF(string _contentPath, DBExtension _db)
        {
            content = _contentPath;
            db = _db;
        }

        private void CabeceraPie(Document _document, Section section, string titulo, bool logo = true)
        {
            try
            {
                TextFrame tf = section.Headers.Primary.AddTextFrame();
                tf.RelativeHorizontal = RelativeHorizontal.Margin;
                tf.Height = 55;
                if(section.PageSetup.Orientation == Orientation.Portrait)
                {
                    tf.Width = section.PageSetup.PageWidth - section.PageSetup.RightMargin - section.PageSetup.LeftMargin;
                }
                else
                {
                    tf.Width = section.PageSetup.PageHeight - section.PageSetup.RightMargin - section.PageSetup.LeftMargin;
                }
                tf.FillFormat.Color = new Color(233, 236, 239);
                if (logo)
                {
                    ImageSource.ImageSourceImpl = new ImageSharpImageSource<Rgba32>();
                    var image = tf.AddImage(ImageSource.FromFile(Path.Combine(content, "img", "logo_mica.png")));                    
                    image.Height = 47;
                    image.Width = 162;
                    image.RelativeVertical = RelativeVertical.Line;
                    image.RelativeHorizontal = RelativeHorizontal.Margin;
                    image.Top = ShapePosition.Top;
                    image.Left = ShapePosition.Left;
                    image.WrapFormat.Style = WrapStyle.Through;
                }
                var pCabecera = tf.AddParagraph();
                pCabecera.Format.SpaceBefore = 20;
                pCabecera.AddText(titulo);
                pCabecera.Format.Font.Size = 11;
                pCabecera.Format.Font.Bold = true;
                pCabecera.Format.Alignment = ParagraphAlignment.Right;
                pCabecera.Format.RightIndent = 5;
                var hrBorder = new Border();
                hrBorder.Width = "1pt";
                var pBorde = tf.AddParagraph();
                pBorde.Format.SpaceBefore = 12;
                pBorde.Format.Borders.Bottom = hrBorder;
                //pie
                var paragraph = section.Footers.Primary.AddParagraph();
                paragraph.AddText(System.DateTime.Now.ToLongDateString());
                paragraph.Format.Font.Size = 9;
                paragraph.Format.Alignment = ParagraphAlignment.Center;
            }
            catch
            {
                throw;
            }
        }

        private Document CreateDocument(string titulo, string autor)
        {
            Document _document = new Document();
            try
            {
                _document.Info.Title = titulo;
                _document.Info.Subject = titulo;
                _document.Info.Author = autor;
                var style = _document.Styles["Normal"];
                style.Font.Name = "Arial";
                style = _document.Styles[StyleNames.Header];
                style.ParagraphFormat.AddTabStop("16cm", TabAlignment.Right);
                style = _document.Styles[StyleNames.Footer];
                style.ParagraphFormat.AddTabStop("8cm", TabAlignment.Center);
                style = _document.Styles.AddStyle("Table", "Normal");
                style.Font.Name = "Arial";
                style.Font.Size = 9;
                style = _document.Styles.AddStyle("Title", "Normal");
                style.Font.Name = "Arial";
                style.Font.Size = 10;
                style.Font.Bold = true;
            }
            catch
            {
                throw;
            }
            return _document;
        }

        public async Task<FileStreamResult> ExportFacturas(List<Ficha> fichas)
        {
            FileStreamResult ret = null;
            try
            {
                DataTable dtEmpresa = await this.getEmpresa(fichas[0].IdSalon);
                string empresa = dtEmpresa.Rows[0]["Nombre"].ToString();
                Document _document = this.CreateDocument(empresa + " - Facturas", empresa);
                var section = _document.AddSection();
                section.PageSetup = _document.DefaultPageSetup.Clone();
                section.PageSetup.TopMargin = "5.25cm";
                this.CabeceraPie(_document, section, "Factura");
                for(int i=0; i <= fichas.Count - 1; i++)
                {
                    this.ExportFicha(section, fichas[i], empresa, dtEmpresa, true);
                    if (i < fichas.Count - 1)
                    {
                        _document.LastSection.AddPageBreak();
                    }
                }
                ret = this.Generar(_document);
            }
            catch {
                throw;
            }
            return ret;
        }

        public async Task<FileStreamResult> ExportFicha(ViewModelFicha f)
        {
            FileStreamResult ret = null;
            try
            {
                DataTable dtEmpresa = await this.getEmpresa(f.Datos.IdSalon);
                string empresa = dtEmpresa.Rows[0]["Nombre"].ToString();
                Document _document = this.CreateDocument(empresa + " - Ticket de caja", empresa);
                var section = _document.AddSection();
                section.PageSetup = _document.DefaultPageSetup.Clone();
                section.PageSetup.TopMargin = "5.25cm";
                this.CabeceraPie(_document, section, "Ticket de caja");
                this.ExportFicha(section, f.Datos, empresa, dtEmpresa);
                ret = this.Generar(_document);
            }
            catch { throw; }
            return ret;
        }

        private void ExportFicha(Section section, Ficha Datos, string empresa, DataTable dtEmpresa, bool factura = false)
        {
            try
            {
                Table _table2 = section.AddTable();
                _table2.Style = "Table";
                _table2.Borders.Color = new Color(255, 255, 255);
                _table2.Borders.Width = 0.25;
                _table2.Borders.Left.Width = 0.5;
                _table2.Borders.Right.Width = 0.5;
                _table2.Rows.LeftIndent = 0;
                var column1 = _table2.AddColumn("3cm");
                column1.Format.Alignment = ParagraphAlignment.Left;
                column1.Shading.Color = new Color(System.Drawing.Color.LightGray.R, System.Drawing.Color.LightGray.G, System.Drawing.Color.LightGray.B);
                var column2 = _table2.AddColumn("4cm");
                column2.Format.Alignment = ParagraphAlignment.Left;
                var column3 = _table2.AddColumn("5cm");
                var column4 = _table2.AddColumn("4cm");
                column4.Format.Alignment = ParagraphAlignment.Left;
                var rowDatos = _table2.AddRow();
                rowDatos.Cells[0].Shading.Color = new Color(255, 255, 255);
                rowDatos.Cells[3].AddParagraph(empresa);
                rowDatos.Cells[3].Format.Font.Bold = true;
                rowDatos = _table2.AddRow();
                if (factura)
                {
                    rowDatos.Cells[0].AddParagraph("Factura: ");
                }
                else
                {
                    rowDatos.Cells[0].AddParagraph("Ficha: ");
                }
                rowDatos.Cells[0].Format.Font.Bold = true;
                rowDatos.Cells[1].AddParagraph(Datos.NFicha);
                rowDatos.Cells[1].Format.Font.Bold = false;
                rowDatos.Cells[3].AddParagraph(dtEmpresa.Rows[0]["CIF"].ToString());
                rowDatos = _table2.AddRow();
                rowDatos.Cells[0].AddParagraph("Fecha: ");
                rowDatos.Cells[0].Format.Font.Bold = true;
                rowDatos.Cells[1].AddParagraph(Datos.Fecha.ToShortDateString());
                rowDatos.Cells[1].Format.Font.Bold = false;
                rowDatos.Cells[3].AddParagraph(dtEmpresa.Rows[0]["Direccion"].ToString());
                rowDatos = _table2.AddRow();
                rowDatos.Cells[0].AddParagraph("Forma de pago: ");
                rowDatos.Cells[0].Format.Font.Bold = true;
                rowDatos.Cells[1].AddParagraph(Datos.FormaPago);
                rowDatos.Cells[1].Format.Font.Bold = false;
                rowDatos.Cells[3].AddParagraph(dtEmpresa.Rows[0]["CP"].ToString() + " " + dtEmpresa.Rows[0]["Ciudad"].ToString());
                //tabla para las líneas de la ficha
                TextFrame _espacio = section.AddTextFrame();
                _espacio.Height = "1.0cm";
                Table _tableL = section.AddTable();
                _tableL.Style = "Table";
                _tableL.Borders.Color = new Color(255, 255, 255);
                _tableL.Borders.Width = 0.25;
                _tableL.Borders.Left.Width = 0.5;
                _tableL.Borders.Right.Width = 0.5;
                _tableL.Rows.LeftIndent = 0;
                var columnL1 = _tableL.AddColumn("1cm");
                columnL1.Format.Alignment = ParagraphAlignment.Center;
                var columnL2 = _tableL.AddColumn("10cm");
                columnL2.Format.Alignment = ParagraphAlignment.Left;
                var columnL3 = _tableL.AddColumn("5cm");
                columnL3.Format.Alignment = ParagraphAlignment.Center;
                var rowLC = _tableL.AddRow();
                rowLC.Format.Font.Bold = true;
                rowLC.Shading.Color = new Color(System.Drawing.Color.LightGray.R, System.Drawing.Color.LightGray.G, System.Drawing.Color.LightGray.B);
                rowLC.Cells[0].AddParagraph("#");
                rowLC.Cells[1].AddParagraph("Servicio/Producto");
                rowLC.Cells[2].AddParagraph("Precio (€)");
                for (int i = 0; i <= Datos.Lineas.Count - 1; i++)
                {
                    var rowL = _tableL.AddRow();
                    rowL.Cells[0].AddParagraph((i + 1).ToString());
                    rowL.Cells[1].AddParagraph(Datos.Lineas[i].CodigoServicio + "-" + Datos.Lineas[i].Descripcion);
                    rowL.Cells[2].AddParagraph(Datos.Lineas[i].Base.ToString("N2"));
                }
                Table _tableT = section.AddTable();
                _tableT.Style = "Table";
                _tableT.Borders.Color = new Color(255, 255, 255);
                _tableT.Borders.Width = 0.25;
                _tableT.Borders.Left.Width = 0.5;
                _tableT.Borders.Right.Width = 0.5;
                _tableT.Rows.LeftIndent = 0;
                var columnT1 = _tableT.AddColumn("1cm");
                columnT1.Format.Alignment = ParagraphAlignment.Center;
                var columnT2 = _tableT.AddColumn("7cm");
                columnT2.Format.Alignment = ParagraphAlignment.Left;
                var columnT3 = _tableT.AddColumn("3cm");
                columnT3.Format.Alignment = ParagraphAlignment.Left;
                var columnT4 = _tableT.AddColumn("5cm");
                columnT4.Format.Alignment = ParagraphAlignment.Center;
                var rowT1 = _tableT.AddRow();
                rowT1.Cells[2].AddParagraph("Base (€): ");
                rowT1.Cells[2].Shading.Color = new Color(System.Drawing.Color.LightGray.R, System.Drawing.Color.LightGray.G, System.Drawing.Color.LightGray.B);
                rowT1.Cells[2].Format.Alignment = ParagraphAlignment.Right;
                rowT1.Cells[3].AddParagraph(Datos.Base.ToString("N2"));
                if (Datos.TotalDescuentos > 0)
                {
                    rowT1 = _tableT.AddRow();
                    rowT1.Cells[2].AddParagraph("Descuentos (€): ");
                    rowT1.Cells[2].Shading.Color = new Color(System.Drawing.Color.LightGray.R, System.Drawing.Color.LightGray.G, System.Drawing.Color.LightGray.B);
                    rowT1.Cells[2].Format.Alignment = ParagraphAlignment.Right;
                    rowT1.Cells[3].AddParagraph(Datos.TotalDescuentos.ToString("N2"));
                }
                rowT1 = _tableT.AddRow();
                rowT1.Cells[2].AddParagraph("Iva (€): ");
                rowT1.Cells[2].Shading.Color = new Color(System.Drawing.Color.LightGray.R, System.Drawing.Color.LightGray.G, System.Drawing.Color.LightGray.B);
                rowT1.Cells[2].Format.Alignment = ParagraphAlignment.Right;
                rowT1.Cells[3].AddParagraph(Datos.Iva.ToString("N2"));
                rowT1 = _tableT.AddRow();
                rowT1.Cells[2].AddParagraph("Total (€): ");
                rowT1.Cells[2].Shading.Color = new Color(System.Drawing.Color.LightGray.R, System.Drawing.Color.LightGray.G, System.Drawing.Color.LightGray.B);
                rowT1.Cells[2].Format.Alignment = ParagraphAlignment.Right;
                rowT1.Cells[2].Format.Font.Bold = true;
                rowT1.Cells[3].AddParagraph(Datos.Total.ToString("N2"));
                rowT1.Cells[3].Format.Font.Bold = true;
            }
            catch { throw; }
        }

        public async Task<FileStreamResult> ExportGestoria(ViewModelGestoria g)
        {
            FileStreamResult ret = null;
            try
            {
                DataTable dtEmpresa = await this.getEmpresa(g.IdSalon);
                string empresa = dtEmpresa.Rows[0]["Nombre"].ToString();
                Document _document = this.CreateDocument(empresa + " - Datos gestoria " + g.Anio.ToString() + Utils.FormatoFecha(g.NMes.ToString()), empresa);
                var section = _document.AddSection();
                section.PageSetup = _document.DefaultPageSetup.Clone();
                section.PageSetup.TopMargin = "5.25cm";
                section.PageSetup.LeftMargin = "1.25cm";
                section.PageSetup.RightMargin = "1.25cm";
                section.PageSetup.Orientation = Orientation.Landscape;
                //Cabecera / pie
                this.CabeceraPie(_document, section, "Gestoría " + g.Anio.ToString() + Utils.FormatoFecha(g.NMes.ToString()));
                //crear la tabla con el inicio de los datos
                Table _table2 = section.AddTable();
                _table2.Style = "Table";
                _table2.Borders.Color = new Color(255, 255, 255);
                _table2.Borders.Width = 0.25;
                _table2.Borders.Left.Width = 0.5;
                _table2.Borders.Right.Width = 0.5;
                _table2.Rows.LeftIndent = 0;
                var column1 = _table2.AddColumn("1cm");
                column1.Format.Alignment = ParagraphAlignment.Left;
                column1.Shading.Color = new Color(System.Drawing.Color.LightGray.R, System.Drawing.Color.LightGray.G, System.Drawing.Color.LightGray.B);
                var column2 = _table2.AddColumn("1cm");
                column2.Format.Alignment = ParagraphAlignment.Left;
                var column3 = _table2.AddColumn("1cm");
                column3.Format.Alignment = ParagraphAlignment.Left;
                column3.Shading.Color = new Color(System.Drawing.Color.LightGray.R, System.Drawing.Color.LightGray.G, System.Drawing.Color.LightGray.B);
                var column4 = _table2.AddColumn("1cm");
                column4.Format.Alignment = ParagraphAlignment.Left;
                var rowDatos = _table2.AddRow();
                rowDatos.Cells[0].AddParagraph("Año: ");
                rowDatos.Cells[0].Format.Font.Bold = true;
                rowDatos.Cells[1].AddParagraph(g.Anio.ToString());
                rowDatos.Cells[1].Format.Font.Bold = false;
                rowDatos.Cells[2].AddParagraph("Mes: ");
                rowDatos.Cells[2].Format.Font.Bold = true;
                rowDatos.Cells[3].AddParagraph(Utils.NombreMes(g.NMes));
                rowDatos.Cells[3].Format.Font.Bold = false;
                //tabla ingresos / gastos / saldo
                TextFrame _espacio = section.AddTextFrame();
                _espacio.Height = "1.0cm";
                Table _tableL = section.AddTable();
                _tableL.Style = "Table";
                _tableL.Borders.Color = new Color(255, 255, 255);
                _tableL.Borders.Width = 0.25;
                _tableL.Borders.Left.Width = 0.5;
                _tableL.Borders.Right.Width = 0.5;
                _tableL.Rows.LeftIndent = 0;
                var columnL1 = _tableL.AddColumn("2.5cm");
                columnL1.Format.Alignment = ParagraphAlignment.Left;
                var columnL2 = _tableL.AddColumn("2cm");
                columnL2.Format.Alignment = ParagraphAlignment.Right;
                var columnL3 = _tableL.AddColumn("3.5cm");
                columnL3.Format.Alignment = ParagraphAlignment.Left;
                var columnL4 = _tableL.AddColumn("2cm");
                columnL4.Format.Alignment = ParagraphAlignment.Right;
                var columnL5 = _tableL.AddColumn("1.5cm");
                columnL5.Format.Alignment = ParagraphAlignment.Left;
                var columnL6 = _tableL.AddColumn("3.25cm");
                columnL6.Format.Alignment = ParagraphAlignment.Left;
                var columnL7 = _tableL.AddColumn("2cm");
                columnL7.Format.Alignment = ParagraphAlignment.Right;
                var columnL8 = _tableL.AddColumn("1.5cm");
                columnL8.Format.Alignment = ParagraphAlignment.Left;
                var columnL9 = _tableL.AddColumn("3cm");
                columnL9.Format.Alignment = ParagraphAlignment.Left;
                var columnL10 = _tableL.AddColumn("2cm");
                columnL10.Format.Alignment = ParagraphAlignment.Right;
                var rowL1 = _tableL.AddRow();
                rowL1.Cells[0].MergeRight = 3;
                rowL1.Cells[0].AddParagraph("Ingresos:");
                rowL1.Cells[0].Format.Font.Bold = true;
                rowL1.Cells[0].Shading.Color = new Color(System.Drawing.Color.LightGray.R, System.Drawing.Color.LightGray.G, System.Drawing.Color.LightGray.B);
                rowL1.Cells[5].MergeRight = 1;
                rowL1.Cells[5].AddParagraph("Gastos:");
                rowL1.Cells[5].Format.Font.Bold = true;
                rowL1.Cells[5].Shading.Color = new Color(System.Drawing.Color.LightGray.R, System.Drawing.Color.LightGray.G, System.Drawing.Color.LightGray.B);
                rowL1.Cells[8].MergeRight = 1;
                rowL1.Cells[8].AddParagraph("Saldo:");
                rowL1.Cells[8].Format.Font.Bold = true;
                rowL1.Cells[8].Shading.Color = new Color(System.Drawing.Color.LightGray.R, System.Drawing.Color.LightGray.G, System.Drawing.Color.LightGray.B);
                var rowL2 = _tableL.AddRow();
                rowL2.Cells[0].AddParagraph("Tarjeta (€):");
                rowL2.Cells[1].AddParagraph(g.Tarjeta.ToString("N2"));
                rowL2.Cells[2].AddParagraph("Iva tarjeta (€):");
                rowL2.Cells[3].AddParagraph(g.IvaT.ToString("N2"));
                rowL2.Cells[5].AddParagraph("Importe (€):");
                rowL2.Cells[6].AddParagraph(g.Gastos.ToString("N2"));
                rowL2.Cells[8].AddParagraph("Neto (€):");
                rowL2.Cells[9].AddParagraph(g.SaldoNeto.ToString("N2"));
                var rowL3 = _tableL.AddRow();
                rowL3.Cells[0].AddParagraph("Efectivo (€):");
                rowL3.Cells[1].AddParagraph(g.Efectivo.ToString("N2"));
                rowL3.Cells[2].AddParagraph("Iva efectivo (€):");
                rowL3.Cells[3].AddParagraph(g.IvaE.ToString("N2"));
                rowL3.Cells[5].AddParagraph("Iva soportado (€):");
                rowL3.Cells[6].AddParagraph(g.IvaSoportado.ToString("N2"));
                rowL3.Cells[8].AddParagraph("Neto-com. (€):");
                rowL3.Cells[9].AddParagraph(g.SaldoNetoCom.ToString("N2"));
                var rowL4 = _tableL.AddRow();
                rowL4.Cells[0].AddParagraph("Total (€):");
                rowL4.Cells[1].AddParagraph(g.TotalIngresos.ToString("N2"));
                rowL4.Cells[2].AddParagraph("Iva repercutido (€):");
                rowL4.Cells[3].AddParagraph(g.IvaRepercutido.ToString("N2"));
                rowL4.Cells[5].AddParagraph("Comisiones (€):");
                rowL4.Cells[6].AddParagraph(g.Comisiones.ToString("N2"));
                //tabla producciones
                TextFrame _espacio2 = section.AddTextFrame();
                _espacio2.Height = "1.0cm";
                Table _tableP = section.AddTable();
                _tableP.Style = "Table";
                _tableP.Borders.Color = new Color(255, 255, 255);
                _tableP.Borders.Width = 0.25;
                _tableP.Borders.Left.Width = 0.5;
                _tableP.Borders.Right.Width = 0.5;
                _tableP.Rows.LeftIndent = 0;
                var columnP1 = _tableP.AddColumn("4cm");
                columnP1.Format.Alignment = ParagraphAlignment.Left;
                var columnP2 = _tableP.AddColumn("1cm");
                columnP2.Format.Alignment = ParagraphAlignment.Center;
                var columnP3 = _tableP.AddColumn("2cm");
                columnP3.Format.Alignment = ParagraphAlignment.Right;
                var columnP4 = _tableP.AddColumn("2cm");
                columnP4.Format.Alignment = ParagraphAlignment.Right;
                var columnP5 = _tableP.AddColumn("1cm");
                columnP5.Format.Alignment = ParagraphAlignment.Center;
                var columnP6 = _tableP.AddColumn("2cm");
                columnP6.Format.Alignment = ParagraphAlignment.Right;
                var columnP7 = _tableP.AddColumn("2cm");
                columnP7.Format.Alignment = ParagraphAlignment.Right;
                var columnP8 = _tableP.AddColumn("1cm");
                columnP8.Format.Alignment = ParagraphAlignment.Center;
                var columnP9 = _tableP.AddColumn("2cm");
                columnP9.Format.Alignment = ParagraphAlignment.Right;
                var columnP10 = _tableP.AddColumn("2cm");
                columnP10.Format.Alignment = ParagraphAlignment.Right;
                var columnP13 = _tableP.AddColumn("1cm");
                columnP13.Format.Alignment = ParagraphAlignment.Center;
                var columnP14 = _tableP.AddColumn("2cm");
                columnP14.Format.Alignment = ParagraphAlignment.Right;
                var columnP15 = _tableP.AddColumn("2cm");
                columnP15.Format.Alignment = ParagraphAlignment.Right;
                var columnP11 = _tableP.AddColumn("2cm");
                columnP11.Format.Alignment = ParagraphAlignment.Right;
                var columnP12 = _tableP.AddColumn("2cm");
                columnP12.Format.Alignment = ParagraphAlignment.Right;
                var rowP1 = _tableP.AddRow();
                rowP1.Cells[1].MergeRight = 2;
                rowP1.Cells[1].AddParagraph("Servicios-Lavado");
                rowP1.Cells[1].Format.Alignment = ParagraphAlignment.Center;
                rowP1.Cells[1].Format.Font.Bold = true;
                rowP1.Cells[1].Shading.Color = new Color(System.Drawing.Color.LightGray.R, System.Drawing.Color.LightGray.G, System.Drawing.Color.LightGray.B);
                rowP1.Cells[4].MergeRight = 2;
                rowP1.Cells[4].AddParagraph("Servicios-Resto");
                rowP1.Cells[4].Format.Alignment = ParagraphAlignment.Center;
                rowP1.Cells[4].Format.Font.Bold = true;
                rowP1.Cells[4].Shading.Color = new Color(System.Drawing.Color.LightGray.R, System.Drawing.Color.LightGray.G, System.Drawing.Color.LightGray.B);
                rowP1.Cells[7].MergeRight = 2;
                rowP1.Cells[7].AddParagraph("Servicios-Técnicos");
                rowP1.Cells[7].Format.Alignment = ParagraphAlignment.Center;
                rowP1.Cells[7].Format.Font.Bold = true;
                rowP1.Cells[7].Shading.Color = new Color(System.Drawing.Color.LightGray.R, System.Drawing.Color.LightGray.G, System.Drawing.Color.LightGray.B);
                rowP1.Cells[10].MergeRight = 2;
                rowP1.Cells[10].AddParagraph("Productos");
                rowP1.Cells[10].Format.Alignment = ParagraphAlignment.Center;
                rowP1.Cells[10].Format.Font.Bold = true;
                rowP1.Cells[10].Shading.Color = new Color(System.Drawing.Color.LightGray.R, System.Drawing.Color.LightGray.G, System.Drawing.Color.LightGray.B);
                rowP1.Cells[13].MergeDown = 1;
                rowP1.Cells[13].AddParagraph("Total " + System.Environment.NewLine + "Prod.(€)");
                rowP1.Cells[13].Format.Alignment = ParagraphAlignment.Center;
                rowP1.Cells[13].Format.Font.Bold = true;
                rowP1.Cells[13].Shading.Color = new Color(System.Drawing.Color.LightGray.R, System.Drawing.Color.LightGray.G, System.Drawing.Color.LightGray.B);
                rowP1.Cells[14].MergeDown = 1;
                rowP1.Cells[14].AddParagraph("Total " + System.Environment.NewLine + "Com.(€)");
                rowP1.Cells[14].Format.Alignment = ParagraphAlignment.Center;
                rowP1.Cells[14].Format.Font.Bold = true;
                rowP1.Cells[14].Shading.Color = new Color(System.Drawing.Color.LightGray.R, System.Drawing.Color.LightGray.G, System.Drawing.Color.LightGray.B);
                var rowP2 = _tableP.AddRow();
                rowP2.Cells[0].AddParagraph("Empleado");
                rowP2.Cells[0].Format.Font.Bold = true;
                rowP2.Cells[0].Shading.Color = new Color(System.Drawing.Color.LightGray.R, System.Drawing.Color.LightGray.G, System.Drawing.Color.LightGray.B);
                rowP2.Cells[1].AddParagraph("#");
                rowP2.Cells[1].Format.Font.Bold = true;
                rowP2.Cells[1].Shading.Color = new Color(System.Drawing.Color.LightGray.R, System.Drawing.Color.LightGray.G, System.Drawing.Color.LightGray.B);
                rowP2.Cells[2].AddParagraph("Prod.(€)");
                rowP2.Cells[2].Format.Font.Bold = true;
                rowP2.Cells[2].Shading.Color = new Color(System.Drawing.Color.LightGray.R, System.Drawing.Color.LightGray.G, System.Drawing.Color.LightGray.B);
                rowP2.Cells[3].AddParagraph("Com.(€)");
                rowP2.Cells[3].Format.Font.Bold = true;
                rowP2.Cells[3].Shading.Color = new Color(System.Drawing.Color.LightGray.R, System.Drawing.Color.LightGray.G, System.Drawing.Color.LightGray.B);
                rowP2.Cells[4].AddParagraph("#");
                rowP2.Cells[4].Format.Font.Bold = true;
                rowP2.Cells[4].Shading.Color = new Color(System.Drawing.Color.LightGray.R, System.Drawing.Color.LightGray.G, System.Drawing.Color.LightGray.B);
                rowP2.Cells[5].AddParagraph("Prod.(€)");
                rowP2.Cells[5].Format.Font.Bold = true;
                rowP2.Cells[5].Shading.Color = new Color(System.Drawing.Color.LightGray.R, System.Drawing.Color.LightGray.G, System.Drawing.Color.LightGray.B);
                rowP2.Cells[6].AddParagraph("Com.(€)");
                rowP2.Cells[6].Format.Font.Bold = true;
                rowP2.Cells[6].Shading.Color = new Color(System.Drawing.Color.LightGray.R, System.Drawing.Color.LightGray.G, System.Drawing.Color.LightGray.B);
                rowP2.Cells[7].AddParagraph("#");
                rowP2.Cells[7].Format.Font.Bold = true;
                rowP2.Cells[7].Shading.Color = new Color(System.Drawing.Color.LightGray.R, System.Drawing.Color.LightGray.G, System.Drawing.Color.LightGray.B);
                rowP2.Cells[8].AddParagraph("Prod.(€)");
                rowP2.Cells[8].Format.Font.Bold = true;
                rowP2.Cells[8].Shading.Color = new Color(System.Drawing.Color.LightGray.R, System.Drawing.Color.LightGray.G, System.Drawing.Color.LightGray.B);
                rowP2.Cells[9].AddParagraph("Com.(€)");
                rowP2.Cells[9].Format.Font.Bold = true;
                rowP2.Cells[9].Shading.Color = new Color(System.Drawing.Color.LightGray.R, System.Drawing.Color.LightGray.G, System.Drawing.Color.LightGray.B);
                rowP2.Cells[10].AddParagraph("#");
                rowP2.Cells[10].Format.Font.Bold = true;
                rowP2.Cells[10].Shading.Color = new Color(System.Drawing.Color.LightGray.R, System.Drawing.Color.LightGray.G, System.Drawing.Color.LightGray.B);
                rowP2.Cells[11].AddParagraph("Prod.(€)");
                rowP2.Cells[11].Format.Font.Bold = true;
                rowP2.Cells[11].Shading.Color = new Color(System.Drawing.Color.LightGray.R, System.Drawing.Color.LightGray.G, System.Drawing.Color.LightGray.B);
                rowP2.Cells[12].AddParagraph("Com.(€)");
                rowP2.Cells[12].Format.Font.Bold = true;
                rowP2.Cells[12].Shading.Color = new Color(System.Drawing.Color.LightGray.R, System.Drawing.Color.LightGray.G, System.Drawing.Color.LightGray.B);
                foreach (GestoriaProduccion p in g.Producciones)
                {
                    var rowP3 = _tableP.AddRow();
                    rowP3.Cells[0].AddParagraph(p.Empleado);
                    rowP3.Cells[1].AddParagraph(p.NServiciosL.ToString("N0"));
                    rowP3.Cells[2].AddParagraph(p.ProdServiciosL.ToString("N2"));
                    rowP3.Cells[3].AddParagraph(p.ComisionServiciosL.ToString("N2"));
                    rowP3.Cells[4].AddParagraph(p.NServiciosR.ToString("N0"));
                    rowP3.Cells[5].AddParagraph(p.ProdServiciosR.ToString("N2"));
                    rowP3.Cells[6].AddParagraph(p.ComisionServiciosR.ToString("N2"));
                    rowP3.Cells[7].AddParagraph(p.NServiciosT.ToString("N0"));
                    rowP3.Cells[8].AddParagraph(p.ProdServiciosT.ToString("N2"));
                    rowP3.Cells[9].AddParagraph(p.ComisionServiciosT.ToString("N2"));
                    rowP3.Cells[10].AddParagraph(p.NProductos.ToString("N0"));
                    rowP3.Cells[11].AddParagraph(p.ProdProductos.ToString("N2"));
                    rowP3.Cells[12].AddParagraph(p.ComisionProductos.ToString("N2"));
                    rowP3.Cells[13].AddParagraph(p.TotalProduccion.ToString("N2"));
                    rowP3.Cells[14].AddParagraph(p.TotalComisiones.ToString("N2"));
                }
                //---------------------------------------------------------------
                ret = this.Generar(_document);
            }
            catch { throw; }
            return ret;
        }

        public FileStreamResult ExportProduccion(string aniomes, GestoriaProduccion p, List<Ficha> lstFichas)
        {
            FileStreamResult ret = null;
            try
            {
                Document _document = this.CreateDocument("Producción - cod " + p.Codigo, "");
                var section = _document.AddSection();
                section.PageSetup = _document.DefaultPageSetup.Clone();
                section.PageSetup.TopMargin = "5.25cm";
                section.PageSetup.LeftMargin = "1.25cm";
                section.PageSetup.RightMargin = "1.25cm";
                //Cabecera / pie
                this.CabeceraPie(_document, section, "Producción " + aniomes + System.Environment.NewLine + "Empleado: " + p.Empleado, false);
                //tabla resumen producciones por tipo
                Table _table2 = section.AddTable();
                _table2.Style = "Table";
                _table2.Borders.Color = new Color(255, 255, 255);
                _table2.Borders.Width = 0.25;
                _table2.Borders.Left.Width = 0.5;
                _table2.Borders.Right.Width = 0.5;
                _table2.Rows.LeftIndent = 0;
                var column1 = _table2.AddColumn("2.5cm");
                column1.Format.Alignment = ParagraphAlignment.Left;
                var column2 = _table2.AddColumn("2cm");
                column2.Format.Alignment = ParagraphAlignment.Right;
                var column3 = _table2.AddColumn("0.2cm");
                column3.Format.Alignment = ParagraphAlignment.Left;
                var column4 = _table2.AddColumn("2.5cm");
                column4.Format.Alignment = ParagraphAlignment.Left;
                var column5 = _table2.AddColumn("2cm");
                column5.Format.Alignment = ParagraphAlignment.Right;
                var column6 = _table2.AddColumn("0.2cm");
                column6.Format.Alignment = ParagraphAlignment.Left;
                var column9 = _table2.AddColumn("2.5cm");
                column9.Format.Alignment = ParagraphAlignment.Left;
                var column10 = _table2.AddColumn("2cm");
                column10.Format.Alignment = ParagraphAlignment.Right;
                var column11 = _table2.AddColumn("0.2cm");
                column11.Format.Alignment = ParagraphAlignment.Left;
                var column7 = _table2.AddColumn("2.5cm");
                column7.Format.Alignment = ParagraphAlignment.Left;
                var column8 = _table2.AddColumn("2cm");
                column8.Format.Alignment = ParagraphAlignment.Right;
                var rowDatos = _table2.AddRow();
                rowDatos.Cells[0].MergeRight = 1;
                rowDatos.Cells[0].AddParagraph("Servicios - Lavado");
                rowDatos.Cells[0].Shading.Color = new Color(System.Drawing.Color.LightGray.R, System.Drawing.Color.LightGray.G, System.Drawing.Color.LightGray.B);
                rowDatos.Cells[0].Format.Font.Bold = true;
                rowDatos.Cells[3].MergeRight = 1;
                rowDatos.Cells[3].AddParagraph("Servicios - Resto");
                rowDatos.Cells[3].Shading.Color = new Color(System.Drawing.Color.LightGray.R, System.Drawing.Color.LightGray.G, System.Drawing.Color.LightGray.B);
                rowDatos.Cells[3].Format.Font.Bold = true;
                rowDatos.Cells[6].MergeRight = 1;
                rowDatos.Cells[6].AddParagraph("Servicios - Técnicos");
                rowDatos.Cells[6].Shading.Color = new Color(System.Drawing.Color.LightGray.R, System.Drawing.Color.LightGray.G, System.Drawing.Color.LightGray.B);
                rowDatos.Cells[6].Format.Font.Bold = true;
                rowDatos.Cells[9].MergeRight = 1;
                rowDatos.Cells[9].AddParagraph("Productos");
                rowDatos.Cells[9].Shading.Color = new Color(System.Drawing.Color.LightGray.R, System.Drawing.Color.LightGray.G, System.Drawing.Color.LightGray.B);
                rowDatos.Cells[9].Format.Font.Bold = true;
                var rowDatos1 = _table2.AddRow();
                rowDatos1.Cells[0].AddParagraph("# Servicios: ");
                rowDatos1.Cells[1].AddParagraph(p.NServiciosL.ToString("N0"));
                rowDatos1.Cells[3].AddParagraph("# Servicios: ");
                rowDatos1.Cells[4].AddParagraph(p.NServiciosR.ToString("N0"));
                rowDatos1.Cells[6].AddParagraph("# Servicios: ");
                rowDatos1.Cells[7].AddParagraph(p.NServiciosT.ToString("N0"));
                rowDatos1.Cells[9].AddParagraph("# Productos: ");
                rowDatos1.Cells[10].AddParagraph(p.NProductos.ToString("N0"));
                var rowDatos2 = _table2.AddRow();
                rowDatos2.Cells[0].AddParagraph("Producción (€): ");
                rowDatos2.Cells[1].AddParagraph(p.ProdServiciosL.ToString("N2"));
                rowDatos2.Cells[3].AddParagraph("Producción (€): ");
                rowDatos2.Cells[4].AddParagraph(p.ProdServiciosR.ToString("N2"));
                rowDatos2.Cells[6].AddParagraph("Producción (€): ");
                rowDatos2.Cells[7].AddParagraph(p.ProdServiciosT.ToString("N2"));
                rowDatos2.Cells[9].AddParagraph("Producción (€): ");
                rowDatos2.Cells[10].AddParagraph(p.ProdProductos.ToString("N2"));
                var rowDatos3 = _table2.AddRow();
                rowDatos3.Cells[0].AddParagraph("Comisiones (€): ");
                rowDatos3.Cells[1].AddParagraph(p.ComisionServiciosL.ToString("N2"));
                rowDatos3.Cells[3].AddParagraph("Comisiones (€): ");
                rowDatos3.Cells[4].AddParagraph(p.ComisionServiciosR.ToString("N2"));
                rowDatos3.Cells[6].AddParagraph("Comisiones (€): ");
                rowDatos3.Cells[7].AddParagraph(p.ComisionServiciosT.ToString("N2"));
                rowDatos3.Cells[9].AddParagraph("Comisiones (€): ");
                rowDatos3.Cells[10].AddParagraph(p.ComisionProductos.ToString("N2"));
                var rowDatos4 = _table2.AddRow();
                rowDatos4.Cells[0].MergeRight = 9;
                rowDatos4.Cells[0].Shading.Color = new Color(System.Drawing.Color.LightGray.R, System.Drawing.Color.LightGray.G, System.Drawing.Color.LightGray.B);
                rowDatos4.Cells[0].AddParagraph("Total comisiones (€): ");
                rowDatos4.Cells[0].Format.Alignment = ParagraphAlignment.Right;
                rowDatos4.Cells[0].Format.Font.Bold = true;
                rowDatos4.Cells[10].Shading.Color = new Color(System.Drawing.Color.LightGray.R, System.Drawing.Color.LightGray.G, System.Drawing.Color.LightGray.B);
                rowDatos4.Cells[10].AddParagraph(p.TotalComisiones.ToString("N2"));
                rowDatos4.Cells[10].Format.Font.Bold = true;
                var rowDatos5 = _table2.AddRow();
                rowDatos5.Cells[0].MergeRight = 9;
                rowDatos5.Cells[0].Shading.Color = new Color(System.Drawing.Color.LightGray.R, System.Drawing.Color.LightGray.G, System.Drawing.Color.LightGray.B);
                rowDatos5.Cells[0].AddParagraph("Total producción (€): ");
                rowDatos5.Cells[0].Format.Alignment = ParagraphAlignment.Right;
                rowDatos5.Cells[0].Format.Font.Bold = true;
                rowDatos5.Cells[10].Shading.Color = new Color(System.Drawing.Color.LightGray.R, System.Drawing.Color.LightGray.G, System.Drawing.Color.LightGray.B);
                rowDatos5.Cells[10].AddParagraph(p.TotalProduccion.ToString("N2"));
                rowDatos5.Cells[10].Format.Font.Bold = true;
                this.ExportarProduccion_Paquetes(_table2, "Paquete 1: ", p.Paquetes.ServicioLE1, p.Paquetes.ServicioLP1, p.Paquetes.ServicioSE1, p.Paquetes.ServicioSP1, p.Paquetes.ServicioTE1, p.Paquetes.ServicioTP1, p.Paquetes.ProductoE1, p.Paquetes.ProductoP1);
                this.ExportarProduccion_Paquetes(_table2, "Paquete 2: ", p.Paquetes.ServicioLE2, p.Paquetes.ServicioLP2, p.Paquetes.ServicioSE2, p.Paquetes.ServicioSP2, p.Paquetes.ServicioTE2, p.Paquetes.ServicioTP2, p.Paquetes.ProductoE2, p.Paquetes.ProductoP2);
                this.ExportarProduccion_Paquetes(_table2, "Paquete 3: ", p.Paquetes.ServicioLE3, p.Paquetes.ServicioLP3, p.Paquetes.ServicioSE3, p.Paquetes.ServicioSP3, p.Paquetes.ServicioTE3, p.Paquetes.ServicioTP3, p.Paquetes.ProductoE3, p.Paquetes.ProductoP3);
                this.ExportarProduccion_Paquetes(_table2, "Paquete 4: ", p.Paquetes.ServicioLE4, p.Paquetes.ServicioLP4, p.Paquetes.ServicioSE4, p.Paquetes.ServicioSP4, p.Paquetes.ServicioTE4, p.Paquetes.ServicioTP4, p.Paquetes.ProductoE4, p.Paquetes.ProductoP4);
                if (lstFichas != null)
                {
                    //tabla listado de fichas incluidas en la produccion
                    TextFrame _espacio2 = section.AddTextFrame();
                    _espacio2.Height = "1.0cm";
                    Table _table3 = section.AddTable();
                    _table3.Style = "Table";
                    _table3.Borders.Color = new Color(255, 255, 255);
                    _table3.Borders.Width = 0.25;
                    _table3.Borders.Left.Width = 0.5;
                    _table3.Borders.Right.Width = 0.5;
                    _table3.Rows.LeftIndent = 0;
                    var columnP1 = _table3.AddColumn("2cm");
                    columnP1.Format.Alignment = ParagraphAlignment.Left;
                    var columnP2 = _table3.AddColumn("2cm");
                    columnP2.Format.Alignment = ParagraphAlignment.Left;
                    var columnP3 = _table3.AddColumn("3.4cm");
                    columnP3.Format.Alignment = ParagraphAlignment.Left;
                    var columnP4 = _table3.AddColumn("1.4cm");
                    columnP4.Format.Alignment = ParagraphAlignment.Center;
                    var columnP5 = _table3.AddColumn("2.75cm");
                    columnP5.Format.Alignment = ParagraphAlignment.Left;
                    var columnP6 = _table3.AddColumn("4.2cm");
                    columnP6.Format.Alignment = ParagraphAlignment.Left;
                    var columnP7 = _table3.AddColumn("2.5cm");
                    columnP7.Format.Alignment = ParagraphAlignment.Right;
                    var rowP2 = _table3.AddRow();
                    rowP2.HeadingFormat = true;
                    rowP2.Cells[0].AddParagraph("Fecha");
                    rowP2.Cells[0].Format.Font.Bold = true;
                    rowP2.Cells[0].Shading.Color = new Color(System.Drawing.Color.LightGray.R, System.Drawing.Color.LightGray.G, System.Drawing.Color.LightGray.B);
                    rowP2.Cells[1].AddParagraph("# Ficha");
                    rowP2.Cells[1].Format.Font.Bold = true;
                    rowP2.Cells[1].Shading.Color = new Color(System.Drawing.Color.LightGray.R, System.Drawing.Color.LightGray.G, System.Drawing.Color.LightGray.B);
                    rowP2.Cells[2].AddParagraph("Cliente");
                    rowP2.Cells[2].Format.Font.Bold = true;
                    rowP2.Cells[2].Shading.Color = new Color(System.Drawing.Color.LightGray.R, System.Drawing.Color.LightGray.G, System.Drawing.Color.LightGray.B);
                    rowP2.Cells[3].AddParagraph("# Línea");
                    rowP2.Cells[3].Format.Font.Bold = true;
                    rowP2.Cells[3].Shading.Color = new Color(System.Drawing.Color.LightGray.R, System.Drawing.Color.LightGray.G, System.Drawing.Color.LightGray.B);
                    rowP2.Cells[4].AddParagraph("Tipo");
                    rowP2.Cells[4].Format.Font.Bold = true;
                    rowP2.Cells[4].Shading.Color = new Color(System.Drawing.Color.LightGray.R, System.Drawing.Color.LightGray.G, System.Drawing.Color.LightGray.B);
                    rowP2.Cells[5].AddParagraph("Descripción");
                    rowP2.Cells[5].Format.Font.Bold = true;
                    rowP2.Cells[5].Shading.Color = new Color(System.Drawing.Color.LightGray.R, System.Drawing.Color.LightGray.G, System.Drawing.Color.LightGray.B);
                    rowP2.Cells[6].AddParagraph("Producción (€)");
                    rowP2.Cells[6].Format.Font.Bold = true;
                    rowP2.Cells[6].Shading.Color = new Color(System.Drawing.Color.LightGray.R, System.Drawing.Color.LightGray.G, System.Drawing.Color.LightGray.B);
                    foreach (Ficha f in lstFichas)
                    {
                        var rowP3 = _table3.AddRow();
                        rowP3.Cells[0].AddParagraph(f.Fecha.ToShortDateString());
                        rowP3.Cells[1].AddParagraph(f.NFicha);
                        rowP3.Cells[2].AddParagraph(f.Cliente);
                        foreach (Ficha_Linea fl in f.Lineas)
                        {
                            rowP3.Cells[3].AddParagraph(fl.Linea.ToString());
                            rowP3.Cells[4].AddParagraph(fl.Tipo);
                            rowP3.Cells[5].AddParagraph(fl.Descripcion);
                            rowP3.Cells[6].AddParagraph(fl.Base.ToString("N2"));
                        }
                    }
                }
                //---------------------------------------------------------------
                ret = this.Generar(_document);
            }
            catch { throw; }
            return ret;
        }

        private void ExportarProduccion_Paquetes(Table _tb, string paquete, double lavadoE, double lavadoP, double serviciosE, double serviciosP, double serviciosET, double serviciosPT, double productosE, double productosP)
        {
            try
            {
                if (lavadoP > 0 || serviciosP > 0 || serviciosPT > 0 || productosP > 0)
                {
                    var rowDatos4 = _tb.AddRow();
                    if (lavadoP > 0 || paquete.Equals("Paquete 1: "))
                    {
                        rowDatos4.Cells[0].AddParagraph(paquete);
                        rowDatos4.Cells[1].AddParagraph(lavadoE.ToString("N2") + System.Environment.NewLine + "(" + lavadoP.ToString("N2") + " %)");
                    }
                    if (serviciosP > 0 || paquete.Equals("Paquete 1: "))
                    {
                        rowDatos4.Cells[3].AddParagraph(paquete);
                        rowDatos4.Cells[4].AddParagraph(serviciosE.ToString("N2") + System.Environment.NewLine + "(" + serviciosP.ToString("N2") + " %)");
                    }
                    if (serviciosPT > 0 || paquete.Equals("Paquete 1: "))
                    {
                        rowDatos4.Cells[6].AddParagraph(paquete);
                        rowDatos4.Cells[7].AddParagraph(serviciosET.ToString("N2") + System.Environment.NewLine + "(" + serviciosPT.ToString("N2") + " %)");
                    }
                    if (productosP > 0 || paquete.Equals("Paquete 1: "))
                    {
                        rowDatos4.Cells[9].AddParagraph(paquete);
                        rowDatos4.Cells[10].AddParagraph(productosE.ToString("N2") + System.Environment.NewLine + "(" + productosP.ToString("N2") + " %)");
                    }
                }
            }
            catch { throw; }
        }

        private FileStreamResult Generar(Document _document)
        {
            FileStreamResult ret = null;
            try
            {
                var pdfRenderer = new PdfDocumentRenderer(true);
                pdfRenderer.Document = _document;
                pdfRenderer.RenderDocument();
                MemoryStream ms = new MemoryStream();
                pdfRenderer.Save(ms, false);
                ret = new FileStreamResult(ms, "application/pdf");
            }
            catch { throw; }
            return ret;
        }

        private async Task<DataTable> getEmpresa(int idSalon)
        {
            DataTable ret = null;
            try
            {
                string sql = "select sal.IdEmpresa, e.Nombre, e.Cif, e.Direccion, e.CP, e.Ciudad, e.Telefono " +
                    "from Salones sal inner join Empresas e on sal.IdEmpresa=e.IdEmpresa " +
                    "where sal.IdSalon=@id";
                System.Data.SqlClient.SqlCommand sc = new System.Data.SqlClient.SqlCommand(sql);
                sc.Parameters.AddWithValue("@id", idSalon);
                DataSet ds = await db.GetDataSet(sc, "emp");
                ret = ds.Tables["emp"];
            }
            catch { throw; }
            return ret;
        }

    }
}
