package com.example.mikaapp;

import androidx.appcompat.app.AppCompatActivity;

import android.database.Cursor;
import android.os.Bundle;
import android.widget.TextView;
import android.widget.Toast;

import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Calendar;
import java.util.Date;
import java.util.List;
import java.util.Locale;
import java.util.TimeZone;

public class sincronizar extends AppCompatActivity {
    TextView txtCorreo;
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_sincronizar);

        txtCorreo = findViewById(R.id.txtCorreo);
        //Intent intent = getIntent();
        txtCorreo.setText(ActiveData.loginData.userData.userName);

        try {
            ClientData data = this.getData();
            Sincronizacion sinc = new Sincronizacion(getString(R.string.api_url), ActiveData.loginData.token, getApplicationContext(), ActiveData.loginData.userData.userName);
            sinc.setData(data);

        } catch (Exception ex){
            Toast.makeText(sincronizar.this, "Error: " + ex.getMessage(), Toast.LENGTH_SHORT).show();
        }

    }

    private ClientData getData() throws Exception {
        try {
            DBManager mDbHelper = new DBManager(getApplicationContext());
            mDbHelper.open();

            ClientData data = new ClientData();
            data.fichas = this.getFichas(mDbHelper);
            data.listaClientes = this.getClientes(mDbHelper);

            mDbHelper.close();

            return data;
        } catch (Exception ex){
            throw ex;
        }
    }

    private List<DatosFicha> getFichas(DBManager mDbHelper) throws Exception {
        try {
            List<DatosFicha> fichas = new ArrayList<>();
            //String nFicha = "";
            DatosFicha fichaActiva = new DatosFicha();
            fichaActiva.nFicha = "";

            String sql = "Select f.Fecha, f.Anio, f.Mes, f.Numero, f.idCliente, f.FormaPago, f.Base, f.DescuentoPorc,\n" +
                    "f.DescuentoImp, f.Descuentos, f.Iva, f.Total, f.Pagado, f.Cambio, c.Nombre as Cliente, fl.Linea,\n" +
                    "fl.Codigo, fl.IdServicio, fl.Descripcion, fl.Base, fl.DescuentoPorc, fl.DescuentoCant, fl.IvaPorc,\n" +
                    "fl.IvaCant, fl.Total, s.Tipo, s.Codigo as CodServicio, emp.Nombre || ' ' || emp.Apellidos as Empleado, f.NFicha \n" +
                    "from Fichas f \n" +
                    "inner join Clientes c on c.idSalon=f.idSalon and c.idCliente=f.idCliente\n" +
                    "inner join Fichas_Lineas fl on fl.idSalon=f.idSalon and fl.NFicha=f.NFicha \n" +
                    "inner join Salones sal on sal.idSalon=f.idSalon\n" +
                    "inner join Servicios s on s.IdServicio=fl.IdServicio and s.IdEmpresa=sal.IdEmpresa \n" +
                    "inner join AspNetUsers emp on emp.Salon=fl.idsalon and emp.Codigo=fl.Codigo\n" +
                    "where f.idSalon=?\n" +
                    "ORDER BY f.NFicha, fl.Linea ";
            Cursor c = mDbHelper.getData(sql, new String[]{String.valueOf(ActiveData.loginData.userData.salon)});
            if(c != null && c.getCount()>0){
                while (c.moveToNext()){
                    if (!fichaActiva.nFicha.equals(c.getString(28))){
                        fichaActiva = new DatosFicha();
                        fichaActiva.lineas = new ArrayList<>();
                        fichas.add(fichaActiva);
                    }

                    if (fichaActiva.lineas.size()==0){
                        fichaActiva.nFicha = c.getString(28);
                        SimpleDateFormat formatter = new SimpleDateFormat("dd/MM/yyyy", Locale.ENGLISH);
                        formatter.setTimeZone(TimeZone.getTimeZone("Europe/Madrid"));
                        Date fec = formatter.parse(c.getString(0));
                        Calendar cal = Calendar.getInstance();
                        cal.setTime(fec);
                        fichaActiva.fecha = String.valueOf(cal.get(Calendar.YEAR)) + "/" + String.valueOf(cal.get(Calendar.MONTH)+1) + "/" + String.valueOf(cal.get(Calendar.DAY_OF_MONTH));
                        fichaActiva.anio = c.getInt(1);
                        fichaActiva.mes = c.getInt(2);
                        fichaActiva.numero = c.getInt(3);
                        fichaActiva.idCliente = c.getInt(4);
                        fichaActiva.formaPago = c.getString(5);
                        fichaActiva.base = c.getFloat(6);
                        fichaActiva.descuentoPorc = c.getFloat(7);
                        fichaActiva.descuentoImp = c.getFloat(8);
                        fichaActiva.descuentos = c.getFloat(9);
                        fichaActiva.iva = c.getFloat(10);
                        fichaActiva.total = c.getFloat(11);
                        fichaActiva.pagado = c.getFloat(12);
                        fichaActiva.cambio = c.getFloat(13);
                        fichaActiva.nombreCliente = c.getString(14);
                        fichaActiva.cerrada = false;
                        fichaActiva.idSalon = ActiveData.loginData.userData.salon;
                    }
                    DatosFichaLinea linea = new DatosFichaLinea();
                    linea.nFicha = c.getString(28);
                    linea.idSalon = ActiveData.loginData.userData.salon;
                    linea.linea = c.getInt(15);
                    linea.codigo = c.getString(16);
                    linea.idServicio = c.getInt(17);
                    linea.descripcion = c.getString(18);
                    linea.base = c.getFloat(19);
                    linea.descuentoPorc = c.getFloat(20);
                    linea.descuentoCant = c.getFloat(21);
                    linea.ivaPorc = c.getFloat(22);
                    linea.ivaCant = c.getFloat(23);
                    linea.total = c.getFloat(24);
                    linea.tipo = c.getString(25);
                    linea.codServicio = c.getString(26);
                    linea.nEmpleado = c.getString(27);

                    fichaActiva.lineas.add(linea);
                }
            }

            return fichas;
        } catch (Exception ex){
            throw ex;
        }
    }

    private List<DatosCliente> getClientes(DBManager mDbHelper){
        try {
            List<DatosCliente> clientes = new ArrayList<>();
            String sql = "SELECT idCliente, Nombre, Telefono, Email from Clientes where idSalon=? and Nuevo=1";
            Cursor c = mDbHelper.getData(sql, new String[]{String.valueOf(ActiveData.loginData.userData.salon)});
            if(c != null && c.getCount()>0){
                while (c.moveToNext()){
                    DatosCliente cliente = new DatosCliente();
                    cliente.idCliente = c.getInt(0);
                    cliente.nombre = c.getString(1);
                    cliente.telefono = c.getString(2);
                    cliente.email = c.getString(3);
                    cliente.idSalon = ActiveData.loginData.userData.salon;
                    cliente.nuevo = true;
                    clientes.add(cliente);
                }
            }

            return clientes;
        } catch (Exception ex){
            throw ex;
        }
    }

}