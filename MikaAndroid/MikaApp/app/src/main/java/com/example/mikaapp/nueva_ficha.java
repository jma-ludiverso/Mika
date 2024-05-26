package com.example.mikaapp;

import androidx.appcompat.app.AppCompatActivity;
import androidx.constraintlayout.widget.ConstraintLayout;
import androidx.fragment.app.DialogFragment;

import android.app.ActionBar;
import android.app.DatePickerDialog;
import android.content.Context;
import android.content.Intent;
import android.os.Bundle;
import android.view.Gravity;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.view.WindowManager;
import android.widget.Button;
import android.widget.DatePicker;
import android.widget.EditText;
import android.widget.ImageButton;
import android.widget.ImageView;
import android.widget.RadioButton;
import android.widget.RadioGroup;
import android.widget.RelativeLayout;
import android.widget.TableLayout;
import android.widget.TableRow;
import android.widget.TextView;
import android.widget.Toast;

import java.text.DecimalFormat;
import java.text.NumberFormat;
import java.text.SimpleDateFormat;
import java.util.Calendar;
import java.util.Currency;
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
        for (DatosFichaLinea linea : lineas) {
            datosLinea(linea);
        }
    }

    public void datosLinea(DatosFichaLinea linea) {
        if (linea.linea == 0){
            linea.linea = ActiveData.Ficha.lineas.size()+1;
            ActiveData.Ficha.lineas.add(linea);
        }
        TableRow fila = new TableRow(this);
        fila.setBackgroundColor(getResources().getColor(R.color.lightgrey));
        fila.setLayoutParams(new TableRow.LayoutParams(TableRow.LayoutParams.MATCH_PARENT, TableRow.LayoutParams.WRAP_CONTENT));

        LayoutInflater inflater = LayoutInflater.from(getApplicationContext());
        ConstraintLayout botones = (ConstraintLayout) inflater.inflate(R.layout.botones_fila,null);
        Button btnModifcar = botones.findViewById(R.id.btnModificar);
        btnModifcar.setTag(linea);
        btnModifcar.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                modificar(v);
            }
        });
        fila.addView(botones, new TableRow.LayoutParams(0, TableRow.LayoutParams.WRAP_CONTENT, 1f));

        TextView emp = new TextView(this);
        emp.setText(linea.codigo + " - " + linea.nEmpleado);
        emp.setTextSize(20);
        emp.setTextColor(getResources().getColor(R.color.black));
        emp.setGravity(Gravity.CENTER);
        fila.addView(emp, new TableRow.LayoutParams(0, TableRow.LayoutParams.WRAP_CONTENT, 1f));

        ImageView tipo = new ImageView(this);
        if(linea.tipo.equals("Producto")){
            tipo.setImageResource(R.drawable.p);
        } else {
            tipo.setImageResource(R.drawable.tijeras);
        }
        tipo.setMaxWidth(50);
        tipo.setMaxHeight(50);
        tipo.setScaleType(ImageView.ScaleType.FIT_CENTER); // Ajustar la escala de la imagen para que quepa dentro del tamaño máximo
        tipo.setAdjustViewBounds(true);
        TableRow.LayoutParams imgParams = new TableRow.LayoutParams(0, TableRow.LayoutParams.WRAP_CONTENT, 1f);
        imgParams.gravity = Gravity.CENTER;
        fila.addView(tipo, imgParams);

        TextView descripcion = new TextView(this);
        descripcion.setText(linea.codServicio + " - " + linea.descripcion);
        descripcion.setTextSize(20);
        descripcion.setTextColor(getResources().getColor(R.color.black));
        descripcion.setGravity(Gravity.CENTER);
        fila.addView(descripcion, new TableRow.LayoutParams(0, TableRow.LayoutParams.WRAP_CONTENT, 1f));

        TextView baseEuros = new TextView(this);

        Currency eur = Currency.getInstance("EUR");
        NumberFormat eurFormatter = NumberFormat.getCurrencyInstance(new Locale("es", "ES"));
        eurFormatter.setCurrency(eur);

        DecimalFormat formatter = (DecimalFormat)NumberFormat.getCurrencyInstance(new Locale("es", "ES"));
        formatter.applyLocalizedPattern("#0,00");
        baseEuros.setText(eurFormatter.format(linea.base));
        baseEuros.setTextSize(20);
        baseEuros.setTextColor(getResources().getColor(R.color.black));
        baseEuros.setGravity(Gravity.CENTER);
        fila.addView(baseEuros, new TableRow.LayoutParams(0, TableRow.LayoutParams.WRAP_CONTENT, 1f));

        TextView descuentoPorcg = new TextView(this);
        descuentoPorcg.setText(formatter.format(linea.descuentoPorc) + " %");
        descuentoPorcg.setTextSize(20);
        descuentoPorcg.setTextColor(getResources().getColor(R.color.black));
        descuentoPorcg.setGravity(Gravity.CENTER);
        fila.addView(descuentoPorcg, new TableRow.LayoutParams(0, TableRow.LayoutParams.WRAP_CONTENT, 1f));

        TextView ivaPorcg = new TextView(this);
        ivaPorcg.setText(formatter.format(linea.ivaPorc) + " %");
        ivaPorcg.setTextSize(20);
        ivaPorcg.setTextColor(getResources().getColor(R.color.black));
        ivaPorcg.setGravity(Gravity.CENTER);
        fila.addView(ivaPorcg, new TableRow.LayoutParams(0, TableRow.LayoutParams.WRAP_CONTENT, 1f));

        TextView totalPorcg = new TextView(this);
        totalPorcg.setText(eurFormatter.format(linea.total));
        totalPorcg.setTextSize(20);
        totalPorcg.setTextColor(getResources().getColor(R.color.black));
        totalPorcg.setGravity(Gravity.CENTER);
        fila.addView(totalPorcg, new TableRow.LayoutParams(0, TableRow.LayoutParams.WRAP_CONTENT, 1f));

        lineas.addView(fila, new TableLayout.LayoutParams(
                ViewGroup.LayoutParams.MATCH_PARENT,
                ViewGroup.LayoutParams.WRAP_CONTENT
        ));

    }

    private void modificar(View v) {
        try {
            DatosFichaLinea linea = (DatosFichaLinea)v.getTag();
            dialog_new_line dlg = new dialog_new_line(nueva_ficha.this, nueva_ficha.this);
            dlg.cargaModificacion(linea);
        } catch (Exception ex){
            Toast.makeText(nueva_ficha.this, "Error: " + ex.getMessage(), Toast.LENGTH_SHORT).show();
        }
    }

}