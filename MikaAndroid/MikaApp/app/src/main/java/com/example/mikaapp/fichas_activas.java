package com.example.mikaapp;

import androidx.appcompat.app.AppCompatActivity;

import android.app.DatePickerDialog;
import android.content.Intent;
import android.database.Cursor;
import android.graphics.Color;
import android.os.Bundle;
import android.view.Gravity;
import android.view.View;
import android.view.ViewGroup;
import android.view.WindowManager;
import android.widget.Button;
import android.widget.DatePicker;
import android.widget.TableLayout;
import android.widget.TableRow;
import android.widget.TextView;
import android.widget.Toast;

import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Calendar;
import java.util.List;
import java.util.Locale;

public class fichas_activas extends AppCompatActivity {
    TextView txtCorreo;
    TextView date;
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_fichas_activas);

        txtCorreo = findViewById(R.id.txtCorreo);
        //Intent intent = getIntent();
        txtCorreo.setText(ActiveData.loginData.userData.userName);

        SimpleDateFormat dateFormat = new SimpleDateFormat("dd/MM/yyyy", Locale.getDefault());

        // Obtener la fecha actual
        Calendar cal = Calendar.getInstance();
        String fechaActual = dateFormat.format(cal.getTime());

        date = findViewById(R.id.txtFechaCalendario);
        date.setText(fechaActual);

        getWindow().setSoftInputMode(WindowManager.LayoutParams.SOFT_INPUT_ADJUST_PAN);
        cargaDatos();
    }

    public void abrirCalendario(View view){
        Calendar cal = Calendar.getInstance();
        int anio = cal.get(Calendar.YEAR);
        int mes = cal.get(Calendar.MONTH);
        int dia = cal.get(Calendar.DAY_OF_MONTH);

        DatePickerDialog dpd = new DatePickerDialog(fichas_activas.this, new DatePickerDialog.OnDateSetListener() {
            @Override
            public void onDateSet(DatePicker view, int year, int month, int dayOfMonth) {
                month = month + 1;
                String fecha = String.format("%02d", dayOfMonth) +"/"+ String.format("%02d", month) +"/"+ year;
                date.setText(fecha);
                cargaDatos();
            }
        }, anio, mes, dia);
        dpd.show();
    }

    private void cargaDatos() {
        try {
            TableLayout tablaLineas = findViewById(R.id.tablaLineas);
            tablaLineas.removeViews(1, tablaLineas.getChildCount() - 1); // Eliminar todas las filas menos la cabecera

            DBManager mDbHelper = new DBManager(getApplicationContext());
            mDbHelper.open();

            String sql = "select f.NFicha, c.Nombre as Cliente, f.Fecha, f.Total " +
                    "from Fichas f " +
                    "inner join Clientes c on c.idSalon=f.IdSalon and c.idCliente=f.idCliente " +
                    "where f.IdSalon=? and f.Fecha=? Order by f.NFicha";
            Cursor c = mDbHelper.getData(sql, new String[]{String.valueOf(ActiveData.loginData.userData.salon), date.getText().toString()});

            if (c != null && c.getCount() > 0) {

                while (c.moveToNext()) {
                    String nFicha = c.getString(0);
                    String cliente = c.getString(1);
                    String fecha = c.getString(2);
                    String total = c.getString(3);

                    TableRow fila = new TableRow(this);
                    fila.setBackgroundColor(getResources().getColor(R.color.lightgrey));
                    fila.setLayoutParams(new TableRow.LayoutParams(TableRow.LayoutParams.MATCH_PARENT, TableRow.LayoutParams.WRAP_CONTENT, 50));

                    Button btnNFicha = new Button(this, null, 0, R.style.btnFilaNFicha);
                    btnNFicha.setText(nFicha);
                    btnNFicha.setGravity(Gravity.CENTER);
                    TableRow.LayoutParams btnParams = new TableRow.LayoutParams(50, TableRow.LayoutParams.WRAP_CONTENT);
                    btnNFicha.setLayoutParams(btnParams);
                    btnNFicha.setOnClickListener(new View.OnClickListener() {
                        @Override
                        public void onClick(View v) {
                            Button btnNFicha = (Button)v;
                            cargaDatosNumeroFicha(btnNFicha.getText().toString());
                        }
                    });
                    fila.addView(btnNFicha);

                    TextView txtCliente = new TextView(this);
                    txtCliente.setText(cliente);
                    txtCliente.setTextSize(20);
                    txtCliente.setTextColor(getResources().getColor(R.color.black));
                    txtCliente.setGravity(Gravity.CENTER);
                    fila.addView(txtCliente, new TableRow.LayoutParams(0, TableRow.LayoutParams.WRAP_CONTENT, 1f));

                    TextView txtFecha = new TextView(this);
                    txtFecha.setText(fecha);
                    txtFecha.setTextSize(20);
                    txtFecha.setTextColor(getResources().getColor(R.color.black));
                    txtFecha.setGravity(Gravity.CENTER);
                    fila.addView(txtFecha, new TableRow.LayoutParams(0, TableRow.LayoutParams.WRAP_CONTENT, 1f));

                    TextView txtTotal = new TextView(this);
                    txtTotal.setText(total);
                    txtTotal.setTextSize(20);
                    txtTotal.setTextColor(getResources().getColor(R.color.black));
                    txtTotal.setGravity(Gravity.CENTER);
                    fila.addView(txtTotal, new TableRow.LayoutParams(0, TableRow.LayoutParams.WRAP_CONTENT, 1f));

                    tablaLineas.addView(fila, new TableLayout.LayoutParams(
                            ViewGroup.LayoutParams.MATCH_PARENT,
                            ViewGroup.LayoutParams.WRAP_CONTENT
                    ));
                }
            } else {
                Toast.makeText(fichas_activas.this, "No se encontraron datos.", Toast.LENGTH_SHORT).show();
            }

            mDbHelper.close();

        } catch (Exception ex) {
            Toast.makeText(fichas_activas.this, "Error: " + ex.getMessage(), Toast.LENGTH_SHORT).show();
        }
    }

    private void cargaDatosNumeroFicha(String nFicha){
        try {
            DBManager mDbHelper = new DBManager(getApplicationContext());
            mDbHelper.open();
            ActiveData.Ficha = new DatosFicha();
            ActiveData.Ficha.lineas = new ArrayList<>();

            String sql = "Select f.Fecha, f.Anio, f.Mes, f.Numero, f.idCliente, f.FormaPago, f.Base, f.DescuentoPorc,\n" +
                    "f.DescuentoImp, f.Descuentos, f.Iva, f.Total, f.Pagado, f.Cambio, c.Nombre as Cliente, fl.Linea,\n" +
                    "fl.Codigo, fl.IdServicio, fl.Descripcion, fl.Base, fl.DescuentoPorc, fl.DescuentoCant, fl.IvaPorc,\n" +
                    "fl.IvaCant, fl.Total, s.Tipo, s.Codigo as CodServicio, emp.Nombre || ' ' || emp.Apellidos as Empleado\n" +
                    "from Fichas f \n" +
                    "inner join Clientes c on c.idSalon=f.idSalon and c.idCliente=f.idCliente\n" +
                    "inner join Fichas_Lineas fl on fl.idSalon=f.idSalon and fl.NFicha=f.NFicha \n" +
                    "inner join Salones sal on sal.idSalon=f.idSalon\n" +
                    "inner join Servicios s on s.IdServicio=fl.IdServicio and s.IdEmpresa=sal.IdEmpresa \n" +
                    "inner join AspNetUsers emp on emp.Salon=fl.idsalon and emp.Codigo=fl.Codigo\n" +
                    "where f.NFicha=? and f.idSalon=?\n" +
                    "ORDER BY fl.Linea ";
            Cursor c = mDbHelper.getData(sql, new String[]{nFicha, String.valueOf(ActiveData.loginData.userData.salon)});
            if(c != null && c.getCount()>0){
                while (c.moveToNext()){
                    if (ActiveData.Ficha.lineas.size()==0){
                        ActiveData.Ficha.nFicha = nFicha;
                        ActiveData.Ficha.nFichaAnterior = nFicha;
                        ActiveData.Ficha.fecha = c.getString(0);
                        ActiveData.Ficha.anio = c.getInt(1);
                        ActiveData.Ficha.mes = c.getInt(2);
                        ActiveData.Ficha.numero = c.getInt(3);
                        ActiveData.Ficha.idCliente = c.getInt(4);
                        ActiveData.Ficha.formaPago = c.getString(5);
                        ActiveData.Ficha.base = c.getFloat(6);
                        ActiveData.Ficha.descuentoPorc = c.getFloat(7);
                        ActiveData.Ficha.descuentoImp = c.getFloat(8);
                        ActiveData.Ficha.descuentos = c.getFloat(9);
                        ActiveData.Ficha.iva = c.getFloat(10);
                        ActiveData.Ficha.total = c.getFloat(11);
                        ActiveData.Ficha.pagado = c.getFloat(12);
                        ActiveData.Ficha.cambio = c.getFloat(13);
                        ActiveData.Ficha.nombreCliente = c.getString(14);
                        ActiveData.Ficha.numero = Integer.parseInt(nFicha);
                        ActiveData.Ficha.idSalon = ActiveData.loginData.userData.salon;
                    }
                    DatosFichaLinea linea = new DatosFichaLinea();
                    linea.nFicha = nFicha;
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
                    ActiveData.Ficha.lineas.add(linea);
                }
            }

            mDbHelper.close();

            Intent i = new Intent(this, nueva_ficha.class);
            startActivity(i);

        } catch (Exception ex){
            Toast.makeText(fichas_activas.this, "Error: " + ex.getMessage(), Toast.LENGTH_SHORT).show();
        }
    }

}