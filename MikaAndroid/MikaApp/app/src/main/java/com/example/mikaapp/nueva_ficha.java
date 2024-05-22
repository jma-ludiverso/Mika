package com.example.mikaapp;

import androidx.appcompat.app.AppCompatActivity;
import androidx.fragment.app.DialogFragment;

import android.app.DatePickerDialog;
import android.content.Context;
import android.content.Intent;
import android.os.Bundle;
import android.view.View;
import android.view.ViewGroup;
import android.view.WindowManager;
import android.widget.Button;
import android.widget.DatePicker;
import android.widget.EditText;
import android.widget.ImageButton;
import android.widget.RadioButton;
import android.widget.RadioGroup;
import android.widget.TableLayout;
import android.widget.TableRow;
import android.widget.TextView;

import java.text.SimpleDateFormat;
import java.util.Calendar;
import java.util.List;
import java.util.Locale;

public class nueva_ficha extends AppCompatActivity {
    TextView date;
    TextView txtCorreo;
    TextView nFicha;
    TextView nombre;
    EditText descuento, totalServicios, totalproductos, base, descuentos, iva, total, pagado, cambio;
    RadioGroup formaPago;
    TableLayout lineas;


    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_nueva_ficha);
        date = findViewById(R.id.txtFechaCalendario);

        txtCorreo = findViewById(R.id.txtCorreo);
        //Intent intent = getIntent();
        txtCorreo.setText(ActiveData.loginData.userData.userName);

        nFicha = findViewById(R.id.txtNumeroFicha);
        nombre = findViewById(R.id.txtNombreCliente);
        descuento = findViewById(R.id.edtxtPorc);
        totalServicios = findViewById(R.id.edtxtServicios);
        totalproductos = findViewById(R.id.edtxtProductos);
        base = findViewById(R.id.edtxtEuro);
        descuentos = findViewById(R.id.edtxtDescuento);
        iva = findViewById(R.id.edtxtIva);
        total = findViewById(R.id.edtxtTotal);
        pagado = findViewById(R.id.edtxtPagado);
        cambio = findViewById(R.id.edtxtCambio);
        formaPago = findViewById(R.id.radio_group);
        lineas = findViewById(R.id.tablaLineas);


        this.cargaDatosFicha();

        getWindow().setSoftInputMode(WindowManager.LayoutParams.SOFT_INPUT_ADJUST_PAN);

        Button button = (Button) findViewById(R.id.b_NuevaLinea);
        button.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                try {
                    dialog_new_line dlg = new dialog_new_line(nueva_ficha.this, nueva_ficha.this);
                } catch (Exception ex){
                    String s = ex.getMessage();
                }

            }
        });

    }

    public void abrirCalendario(View view){
        Calendar cal = Calendar.getInstance();
        int anio = cal.get(Calendar.YEAR);
        int mes = cal.get(Calendar.MONTH);
        int dia = cal.get(Calendar.DAY_OF_MONTH);

        DatePickerDialog dpd = new DatePickerDialog(nueva_ficha.this, new DatePickerDialog.OnDateSetListener() {
            @Override
            public void onDateSet(DatePicker view, int year, int month, int dayOfMonth) {
                month = month + 1;
                String fecha = dayOfMonth +"/"+ month +"/"+ year;
                date.setText(fecha);
            }
        }, anio, mes, dia);
        dpd.show();
    }

    private void cargaDatosFicha(){
        nFicha.setText(ActiveData.Ficha.nFicha);
        date.setText(ActiveData.Ficha.fecha);
        nombre.setText(ActiveData.Ficha.nombreCliente);
        descuento.setText(String.valueOf(ActiveData.Ficha.descuentoPorc));
        descuentos.setText(String.valueOf(ActiveData.Ficha.descuentos));
        //totalServicios.setText(String.valueOf(ActiveData.Ficha));
        //totalproductos.setText(String.valueOf(ActiveData.Ficha.));
        base.setText(String.valueOf(ActiveData.Ficha.base));
        iva.setText(String.valueOf(ActiveData.Ficha.iva));
        total.setText(String.valueOf(ActiveData.Ficha.total));
        pagado.setText(String.valueOf(ActiveData.Ficha.pagado));
        cambio.setText(String.valueOf(ActiveData.Ficha.cambio));
        if (ActiveData.Ficha.formaPago.equals("Tarjeta")){
            formaPago.check(R.id.rb_Tarjeta);
        } else {
            formaPago.check(R.id.rb_Efectivo);
        }
        this.cargaDatosLineas(ActiveData.Ficha.lineas);
    }

    private void cargaDatosLineas(List<DatosFichaLinea> lineas) {

    }

    public void datosLinea(DatosFichaLinea linea){
        TableRow fila = new TableRow(this);
        fila.setBackgroundColor(getResources().getColor(R.color.lightgrey));
        fila.setLayoutParams(new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MATCH_PARENT, ViewGroup.LayoutParams.WRAP_CONTENT));

        TextView txtNum = new TextView(this);
        txtNum.setText("linea1");
        fila.addView(txtNum);
        TextView emp = new TextView(this);
        emp.setText(linea.codigo);
        fila.addView(emp);

        lineas.addView(fila, new TableLayout.LayoutParams());
    }

        /*public void mostrarDialogoNuevaLinea(View view) {
            Intent i = new Intent(this, layout_nueva_linea.class);
            startActivity(i);
        }*/
}