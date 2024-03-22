using Microsoft.AspNetCore.Mvc;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace MikaWeb.Extensions
{
    public class ExportXLS 
    {

        Export conf = null;

        IWorkbook libro = null;
        ISheet hoja = null;
        ICellStyle estiloN = null;
        ICellStyle estiloF = null;
        ICellStyle estiloF2 = null;

        public ExportXLS(Export _conf)
        {
            conf = _conf;
        }

        private void Cabecera()
        {
            try
            {
                IRow Cabecera1 = hoja.CreateRow(0);
                int icolumna = 0;
                for (int i=0; i <= conf.Columnas.Count - 1; i++)
                {
                    Cabecera1.CreateCell(icolumna).SetCellValue(conf.Columnas[i].Header);
                    Cabecera1.Cells[icolumna].CellStyle = conf.EstiloCabecera;
                    hoja.SetColumnWidth(icolumna, conf.Columnas[i].Ancho);
                    icolumna += 1;
                }
            }
            catch { throw; }
        }

        private void Crea_Libro_Hoja()
        {
            try
            {
                libro = new XSSFWorkbook();
                hoja = libro.CreateSheet(conf.NombreHoja);

                IFont fBase = libro.CreateFont();
                fBase.FontName = "Calibri";
                fBase.FontHeightInPoints = 11;

                ICellStyle estiloB = libro.CreateCellStyle();
                estiloB.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Left;
                estiloB.SetFont(fBase);

                ICellStyle estiloCent = libro.CreateCellStyle();
                estiloCent.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;
                estiloCent.SetFont(fBase);

                IFont fNegrita = libro.CreateFont();
                fNegrita.FontName = "Calibri";
                fNegrita.FontHeightInPoints = 11;
                fNegrita.IsBold = true;

                ICellStyle estiloC = libro.CreateCellStyle();
                estiloC.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;
                estiloC.SetFont(fNegrita);

                conf.EstiloBase = estiloB;
                conf.EstiloCabecera = estiloC;
                conf.EstiloCentrado = estiloCent;

            }
            catch { throw; }
        }

        private List<Export_Column> Cols_Tabla(DataTable source)
        {
            List<Export_Column> ret = new List<Export_Column>();
            try
            {
                for (int i = 0; i <= source.Columns.Count - 1; i++)
                {
                    Export_Column col = new Export_Column();
                    col.Ancho = 4000;
                    col.Name = source.Columns[i].ColumnName;
                    col.Header = col.Name;
                    col.Tipo = source.Columns[i].DataType;
                    ret.Add(col);
                }
            }
            catch { throw; }
            return ret;

        }

        public FileStreamResult Export(DataTable source)
        {
            try
            {
                if (conf.Columnas == null)
                {
                    conf.Columnas = this.Cols_Tabla(source);
                }
                this.Crea_Libro_Hoja();
                this.Cabecera();
                this.Lineas(source);
                return this.Guardar();
            }
            catch 
            {
                try
                {
                    libro.Close();
                }
                catch { }
                throw;
            }
        }

        public FileStreamResult Export(List<DataTable> lst, List<string> lstHojas)
        {
            try
            {
                this.Crea_Libro_Hoja();
                for (int i=0; i <= lst.Count - 1; i++)
                {
                    if (i > 0)
                    {
                        hoja = libro.CreateSheet(lstHojas[i]);
                    }
                    conf.Columnas = this.Cols_Tabla(lst[i]);
                    this.Cabecera();
                    this.Lineas(lst[i]);
                }
                return this.Guardar();
            }
            catch
            {
                try
                {
                    libro.Close();
                }
                catch { }
                throw;
            }
        }

        private FileStreamResult Guardar()
        {
            FileStreamResult ret = null;
            try
            {
                MemoryStream fs = new MemoryStream();
                libro.Write(fs, false);
                MemoryStream fs2 = new MemoryStream(fs.ToArray());
                ret = new FileStreamResult(fs2, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet") { FileDownloadName = conf.NombreArchivo};
            }
            catch
            {
                throw;
            }
            return ret;
        }

        private void Linea_Celda(IRow filaExc, Type tipo, string formato, int icolumna, object val)
        {
            try
            {
                switch (tipo.ToString())
                {
                    case "System.String":
                        filaExc.CreateCell(icolumna).SetCellValue(val.ToString());
                        filaExc.Cells[icolumna].CellStyle = conf.EstiloBase;
                        break;
                    case "System.Byte":
                    case "System.Boolean":
                        if (val.ToString().Equals("0") || val.ToString().Equals("1"))
                        {
                            val = val.ToString().Replace("0", "false").Replace("1", "true");
                        }
                        bool b = bool.Parse(val.ToString());
                        if (b)
                        {
                            filaExc.CreateCell(icolumna).SetCellValue("S");
                        }
                        else
                        {
                            filaExc.CreateCell(icolumna).SetCellValue("N");
                        }
                        filaExc.Cells[icolumna].CellStyle = conf.EstiloCentrado;
                        break;
                    case "System.DateTime":
                        filaExc.CreateCell(icolumna);
                        if (!string.IsNullOrEmpty(val.ToString()))
                        {
                            if (string.IsNullOrEmpty(formato))
                            {
                                if (estiloF == null)
                                {
                                    estiloF = libro.CreateCellStyle();
                                    estiloF.SetFont(conf.EstiloBase.GetFont(libro));
                                    estiloF.DataFormat = libro.CreateDataFormat().GetFormat("dd/MM/yyyy");
                                    estiloF.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;
                                }
                                filaExc.Cells[icolumna].CellStyle = estiloF;
                            }
                            else
                            {
                                if (estiloF2 == null)
                                {
                                    estiloF2 = libro.CreateCellStyle();
                                    estiloF2.SetFont(conf.EstiloBase.GetFont(libro));
                                    estiloF2.DataFormat = libro.CreateDataFormat().GetFormat("dd/MM/yyyy HH:mm:ss");
                                    estiloF2.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;
                                }
                                filaExc.Cells[icolumna].CellStyle = estiloF2;
                                val = this.Linea_Formato(formato, val.ToString());
                            }
                            filaExc.Cells[icolumna].SetCellValue(DateTime.Parse(val.ToString()));
                        }
                        break;
                    case "System.Int16":
                    case "System.Int32":
                    case "System.Decimal":
                    case "System.Double":
                        if (estiloN == null)
                        {                            
                            estiloN = libro.CreateCellStyle();
                            estiloN.SetFont(conf.EstiloBase.GetFont(libro));
                            estiloN.DataFormat = HSSFDataFormat.GetBuiltinFormat("#.##");
                            estiloN.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;
                        }
                        switch (tipo.ToString())
                        {
                            case "System.Int16":
                            case "System.Int32":
                                filaExc.CreateCell(icolumna).SetCellValue(int.Parse(val.ToString()));
                                break;
                            case "System.Decimal":
                                filaExc.CreateCell(icolumna).SetCellValue(decimal.ToDouble(decimal.Parse(val.ToString())));
                                break;
                            case "System.Double":
                                filaExc.CreateCell(icolumna).SetCellValue(double.Parse(val.ToString()));
                                break;
                        }
                        filaExc.Cells[icolumna].CellStyle = estiloN;
                        break;
                    default:
                        filaExc.CreateCell(icolumna).SetCellValue(val.ToString());
                        filaExc.Cells[icolumna].CellStyle = conf.EstiloBase;
                        break;
                }
            }
            catch {
                throw;
            }
        }

        private string Linea_Formato(string formato, string data)
        {
            string ret = "";
            try
            {
                string[] separadores = formato.Split('|');
                for(int i=0; i <= separadores.Length - 1; i++)
                {
                    string val = separadores[i];
                    if (val.StartsWith("["))
                    {
                        val = val.Substring(1, val.Length - 2);
                        string[] corte = val.Split(',');
                        ret += data.Substring(int.Parse(corte[0]), int.Parse(corte[1]));
                    }
                    else
                    {
                        ret += val;
                    }
                }
            }
            catch { throw; }
            return ret;
        }

        private void Lineas(DataTable dt)
        {
            try
            {
                for (int i = 0; i <= dt.Rows.Count - 1; i++)
                {
                    IRow filaExc = hoja.CreateRow(i + 1);
                    int icolumna = 0;
                    for (int c = 0; c <= conf.Columnas.Count - 1; c++)
                    {
                        this.Linea_Celda(filaExc, conf.Columnas[c].Tipo, conf.Columnas[c].Formatear, icolumna, dt.Rows[i][conf.Columnas[c].Name]);
                        icolumna += 1;
                    }
                }
            }
            catch { throw; }
        }

    }
}
