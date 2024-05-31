package com.example.mikaapp;

import androidx.appcompat.app.AppCompatActivity;
import androidx.constraintlayout.widget.ConstraintLayout;

import android.app.DatePickerDialog;
import android.content.Intent;
import android.database.Cursor;
import android.os.Bundle;
import android.text.Editable;
import android.text.TextWatcher;
import android.view.Gravity;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.view.WindowManager;
import android.widget.Button;
import android.widget.DatePicker;
import android.widget.EditText;
import android.widget.ImageView;
import android.widget.RadioButton;
import android.widget.RadioGroup;
import android.widget.Spinner;
import android.widget.TableLayout;
import android.widget.TableRow;
import android.widget.TextView;
import android.widget.Toast;

import java.math.BigDecimal;
import java.math.RoundingMode;
import java.text.DecimalFormat;
import java.text.NumberFormat;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.Calendar;
import java.util.Currency;
import java.util.List;
import java.util.Locale;

public class nueva_ficha extends AppCompatActivity {
    TextView date;
    TextView txtCorreo;
    TextView nFicha;
    TextView nombreCliente;
    EditText descuento, totalServicios, totalproductos, base, descuentos, iva, total, pagado, cambio;
    RadioGroup formaPago;
    TableLayout lineas;

    DBManager dbManager;
    List<DatosFichaLinea> listaFichasLineas;
    EditText descuentoPorc;


    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_nueva_ficha);
        date = findViewById(R.id.txtFechaCalendario);
        nFicha = findViewById(R.id.txtNumeroFicha);
        txtCorreo = findViewById(R.id.txtCorreo);
        txtCorreo.setText(ActiveData.loginData.userData.userName);
        nombreCliente = findViewById(R.id.txtNombreCliente);
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

        dbManager = new DBManager(this);
        dbManager.open();

        // Inicializar lista de líneas de fichas (esto es solo un ejemplo, reemplaza con tus datos)
        listaFichasLineas = new ArrayList<>();

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

        Button bGuardarCerrarFicha = findViewById(R.id.b_GuardarCerrarFicha);
        bGuardarCerrarFicha.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                guardarFicha();
            }
        });

        RadioGroup formaPago = findViewById(R.id.radio_group);
        RadioButton rbTarjeta = findViewById(R.id.rb_Tarjeta);
        RadioButton rbEfectivo = findViewById(R.id.rb_Efectivo);
        EditText pagado = findViewById(R.id.edtxtPagado);

        // Agrega el listener al RadioGroup
        formaPago.setOnCheckedChangeListener(new RadioGroup.OnCheckedChangeListener() {
            @Override
            public void onCheckedChanged(RadioGroup group, int checkedId) {
                if (checkedId == R.id.rb_Efectivo) {
                    pagado.setEnabled(true);
                } else if (checkedId == R.id.rb_Tarjeta) {
                    pagado.setEnabled(false);
                }
            }
        });

        descuento.addTextChangedListener(new TextWatcher() {
            @Override
            public void beforeTextChanged(CharSequence s, int start, int count, int after) {

            }

            @Override
            public void onTextChanged(CharSequence s, int start, int before, int count) {
                DecimalFormat formatter = (DecimalFormat)NumberFormat.getCurrencyInstance(new Locale("es", "ES"));
                formatter.applyLocalizedPattern("#0,00");
                calculaTotales(formatter);
                for (DatosFichaLinea linea : ActiveData.Ficha.lineas) {
                    datosLinea(linea, false);
                }
            }

            @Override
            public void afterTextChanged(Editable s) {

            }
        });

        pagado.addTextChangedListener(new TextWatcher() {
            @Override
            public void beforeTextChanged(CharSequence s, int start, int count, int after) {

            }

            @Override
            public void onTextChanged(CharSequence s, int start, int before, int count) {
                calculaCambio();
            }

            @Override
            public void afterTextChanged(Editable s) {

         }
    });

        // Establece el estado inicial del EditText de pagado
        if (formaPago.getCheckedRadioButtonId() == R.id.rb_Efectivo) {
            pagado.setEnabled(true);
        } else {
            pagado.setEnabled(false);
        }

    }

    private void guardarFicha() {
        try {
            DBManager mDbHelper = new DBManager(getApplicationContext());
            mDbHelper.open();
            List<DatosFicha> fichas = new ArrayList<DatosFicha>();
            ActiveData.Ficha.cambio = Float.parseFloat(cambio.getText().toString().replace(",", "."));
            ActiveData.Ficha.descuentoPorc = Float.parseFloat(descuento.getText().toString().replace(",", "."));
            ActiveData.Ficha.base = Float.parseFloat(base.getText().toString().replace(",", "."));
            ActiveData.Ficha.descuentos = Float.parseFloat(descuentos.getText().toString().replace(",", "."));
            ActiveData.Ficha.iva = Float.parseFloat(iva.getText().toString().replace(",", "."));
            ActiveData.Ficha.total = Float.parseFloat(total.getText().toString().replace(",", "."));
            ActiveData.Ficha.pagado = Float.parseFloat(pagado.getText().toString().replace(",", "."));
            ActiveData.Ficha.cambio = Float.parseFloat(cambio.getText().toString().replace(",", "."));

            if (formaPago.getCheckedRadioButtonId()==R.id.rb_Efectivo) {
                ActiveData.Ficha.formaPago ="Efectivo";
            } else if (formaPago.getCheckedRadioButtonId()==R.id.rb_Tarjeta) {
                ActiveData.Ficha.formaPago ="Tarjeta";
            }

            fichas.add(ActiveData.Ficha);
            mDbHelper.insertFichas(fichas);
            mDbHelper.close();
            ActiveData.Ficha = null;
            Intent intent = new Intent(nueva_ficha.this, menu_principal.class);
            startActivity(intent);
            finish();
        } catch (Exception ex) {
            Toast.makeText(nueva_ficha.this, "Error: " + ex.getMessage(), Toast.LENGTH_SHORT).show();
        }
    }

    public void abrirCalendarioFicha(View view){
        Calendar cal = Calendar.getInstance();
        int anio = cal.get(Calendar.YEAR);
        int mes = cal.get(Calendar.MONTH);
        int dia = cal.get(Calendar.DAY_OF_MONTH);

        DatePickerDialog dpd = new DatePickerDialog(nueva_ficha.this, new DatePickerDialog.OnDateSetListener() {
            @Override
            public void onDateSet(DatePicker view, int year, int month, int dayOfMonth) {
                month = month + 1;
                date.setText(String.format("%02d", dayOfMonth) +"/"+ String.format("%02d", month) +"/"+ year);
                ActiveData.Ficha.anio = year;
                ActiveData.Ficha.mes = month;
                ActiveData.Ficha.numero = numeroFicha(ActiveData.Ficha.anio, ActiveData.Ficha.mes);
                ActiveData.Ficha.nFicha = String.valueOf(ActiveData.Ficha.anio) + String.format("%02d", ActiveData.Ficha.mes) + String.format("%03d", ActiveData.Ficha.numero);
                nFicha.setText(ActiveData.Ficha.nFicha);
                for (DatosFichaLinea linea : ActiveData.Ficha.lineas) {
                    linea.nFicha = ActiveData.Ficha.nFicha;
                }
            }
        }, anio, mes, dia);
        dpd.show();
    }

    private int numeroFicha (int anio, int mes){
        try {
            DBManager mDbHelper = new DBManager(getApplicationContext());
            mDbHelper.open();

            Cursor c = mDbHelper.getData("SELECT COALESCE(MAX(Numero), 0) + 1 FROM Fichas WHERE anio=" + anio + " AND mes=" + mes, null);
            c.moveToFirst();
            int numero = c.getInt(0);

            mDbHelper.close();
            return numero;

        } catch (Exception ex){
            throw ex;
        }
    }

    private void calculaCambio(){
        try{
            DecimalFormat formatter = (DecimalFormat)NumberFormat.getCurrencyInstance(new Locale("es", "ES"));
            formatter.applyLocalizedPattern("#0,00");
            float fTotal = Float.parseFloat(total.getText().toString().replace(",", "."));
            float fPagado = Float.parseFloat(pagado.getText().toString().replace(",", "."));
            float fCambio = fPagado - fTotal;
            cambio.setText(formatter.format(fCambio));
        }catch (Exception ex){
            Toast.makeText(nueva_ficha.this, "Error: " + ex.getMessage(), Toast.LENGTH_SHORT).show();
        }
    }

    private void cargaDatosFicha(){
        nFicha.setText(ActiveData.Ficha.nFicha);
        date.setText(ActiveData.Ficha.fecha);
        nombreCliente.setText(ActiveData.Ficha.nombreCliente);
        DecimalFormat formatter = (DecimalFormat)NumberFormat.getCurrencyInstance(new Locale("es", "ES"));
        formatter.applyLocalizedPattern("#0,00");

        descuento.setText(formatter.format(ActiveData.Ficha.descuentoPorc));
        pagado.setText(formatter.format(ActiveData.Ficha.pagado));
        cambio.setText(formatter.format(ActiveData.Ficha.cambio));
        if (ActiveData.Ficha.formaPago.equals("Tarjeta")){
            formaPago.check(R.id.rb_Tarjeta);
        } else {
            formaPago.check(R.id.rb_Efectivo);
        }
        this.cargaDatosLineas(ActiveData.Ficha.lineas);
        this.calculaTotales(formatter);
    }

    private void cargaDatosLineas(List<DatosFichaLinea> lineas) {
        for (DatosFichaLinea linea : lineas) {
            datosLinea(linea, true);
        }
    }

    private float round(float d, int decimalPlace) {
        BigDecimal bd = new BigDecimal(Float.toString(d));
        bd = bd.setScale(decimalPlace, RoundingMode.HALF_UP);
        return bd.floatValue();
    }

    private void calculaTotales(DecimalFormat formatter){
        try{
            float fDescuentoCabecera = 0;
            float fbase = 0;
            float fdescuentos = 0;
            float fiva = 0;
            float ftotal = 0;
            float fservicios = 0;
            float fproductos = 0;
            if (!descuento.getText().toString().isEmpty())
            {
                fDescuentoCabecera = Float.parseFloat(descuento.getText().toString().replace(",", "."));
            }
            for (DatosFichaLinea linea: ActiveData.Ficha.lineas) {
                fbase += linea.base;
                if(fDescuentoCabecera>0 && linea.tipo.equals("Servicio")){
                    float descCant = linea.base * (linea.descuentoPorc / 100);
                    descCant += (linea.base - descCant) * (fDescuentoCabecera / 100);
                    linea.descuentoCant = this.round(descCant, 2);
                    linea.ivaCant = this.round((linea.base - linea.descuentoCant) * (linea.ivaPorc / 100), 3);
                    linea.total = linea.base - linea.descuentoCant + linea.ivaCant;
                }
                fdescuentos += linea.descuentoCant;
                fiva += linea.ivaCant;
                ftotal += linea.total;
                if(linea.tipo.equals("Producto")){
                    fproductos += linea.total;
                }else{
                    fservicios += linea.total;
                }
            }
            base.setText(formatter.format(fbase));
            descuentos.setText(formatter.format(fdescuentos));
            iva.setText(formatter.format(fiva));
            total.setText(formatter.format(ftotal));
            EditText edtxtServicios = findViewById(R.id.edtxtServicios);
            edtxtServicios.setText(formatter.format(fservicios));
            EditText edtxtProductos = findViewById(R.id.edtxtProductos);
            edtxtProductos.setText(formatter.format(fproductos));

        }catch (Exception ex){
            Toast.makeText(nueva_ficha.this, "Error: " + ex.getMessage(), Toast.LENGTH_SHORT).show();
        }
    }

    public void datosLinea(DatosFichaLinea linea, boolean add) {
        Currency eur = Currency.getInstance("EUR");
        NumberFormat eurFormatter = NumberFormat.getCurrencyInstance(new Locale("es", "ES"));
        eurFormatter.setCurrency(eur);

        DecimalFormat formatter = (DecimalFormat)NumberFormat.getCurrencyInstance(new Locale("es", "ES"));
        formatter.applyLocalizedPattern("#0,00");

        if (linea.linea == 0){
            linea.linea = this.getNLinea(ActiveData.Ficha.lineas);
            ActiveData.Ficha.lineas.add(linea);
            this.datosLinea_Add(linea, eurFormatter, formatter);
        } else {
            if(add){
                this.datosLinea_Add(linea, eurFormatter, formatter);
            }else{
                this.datosLinea_Edit(linea, eurFormatter, formatter);
            }
        }
        this.calculaTotales(formatter);
    }


    private void datosLinea_Add(DatosFichaLinea linea, NumberFormat eurFormatter, DecimalFormat formatter) {
        TableRow fila = new TableRow(this);
        fila.setTag(linea.linea);
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
        Button btnEliminar = botones.findViewById(R.id.btnEliminar);
        btnEliminar.setTag(linea);
        btnEliminar.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                eliminar(v);
            }
        });
        fila.addView(botones, new TableRow.LayoutParams(0, TableRow.LayoutParams.WRAP_CONTENT, 1f));

        TextView emp = new TextView(this);
        emp.setTag("empleado");
        emp.setText(linea.codigo + " - " + linea.nEmpleado);
        emp.setTextSize(20);
        emp.setTextColor(getResources().getColor(R.color.black));
        emp.setGravity(Gravity.CENTER);
        fila.addView(emp, new TableRow.LayoutParams(0, TableRow.LayoutParams.WRAP_CONTENT, 1f));

        ImageView tipo = new ImageView(this);
        tipo.setTag("tipo");
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
        descripcion.setTag("descripcion");
        descripcion.setText(linea.codServicio + " - " + linea.descripcion);
        descripcion.setTextSize(20);
        descripcion.setTextColor(getResources().getColor(R.color.black));
        descripcion.setGravity(Gravity.CENTER);
        fila.addView(descripcion, new TableRow.LayoutParams(0, TableRow.LayoutParams.WRAP_CONTENT, 1f));

        TextView baseEuros = new TextView(this);
        baseEuros.setTag("baseeuros");
        baseEuros.setText(eurFormatter.format(linea.base));
        baseEuros.setTextSize(20);
        baseEuros.setTextColor(getResources().getColor(R.color.black));
        baseEuros.setGravity(Gravity.CENTER);
        fila.addView(baseEuros, new TableRow.LayoutParams(0, TableRow.LayoutParams.WRAP_CONTENT, 1f));

        TextView descuentoPorcg = new TextView(this);
        descuentoPorcg.setTag("descuento");
        descuentoPorcg.setText(formatter.format(linea.descuentoPorc) + " %");
        descuentoPorcg.setTextSize(20);
        descuentoPorcg.setTextColor(getResources().getColor(R.color.black));
        descuentoPorcg.setGravity(Gravity.CENTER);
        fila.addView(descuentoPorcg, new TableRow.LayoutParams(0, TableRow.LayoutParams.WRAP_CONTENT, 1f));

        TextView ivaPorcg = new TextView(this);
        ivaPorcg.setTag("iva");
        ivaPorcg.setText(formatter.format(linea.ivaPorc) + " %");
        ivaPorcg.setTextSize(20);
        ivaPorcg.setTextColor(getResources().getColor(R.color.black));
        ivaPorcg.setGravity(Gravity.CENTER);
        fila.addView(ivaPorcg, new TableRow.LayoutParams(0, TableRow.LayoutParams.WRAP_CONTENT, 1f));

        TextView totalPorcg = new TextView(this);
        totalPorcg.setTag("total");
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

    private void datosLinea_Edit(DatosFichaLinea linea, NumberFormat eurFormatter, DecimalFormat formatter) {
        for(int i=0;i<=lineas.getChildCount()-1;i++){
            View vLinea = lineas.getChildAt(i);
            if(vLinea.getTag()!=null){
                if(Integer.parseInt(vLinea.getTag().toString())==linea.linea){
                    Button btnModifcar = vLinea.findViewById(R.id.btnModificar);
                    btnModifcar.setTag(linea);
                    Button btnEliminar = vLinea.findViewById(R.id.btnEliminar);
                    btnEliminar.setTag(linea);
                    TextView emp = vLinea.findViewWithTag("empleado");
                    emp.setText(linea.codigo + " - " + linea.nEmpleado);

                    ImageView tipo = vLinea.findViewWithTag("tipo");
                    if(linea.tipo.equals("Producto")){
                        tipo.setImageResource(R.drawable.p);
                    } else {
                        tipo.setImageResource(R.drawable.tijeras);
                    }

                    TextView descripcion = vLinea.findViewWithTag("descripcion");
                    descripcion.setText(linea.codServicio + " - " + linea.descripcion);

                    TextView baseEuros = vLinea.findViewWithTag("baseeuros");
                    baseEuros.setText(eurFormatter.format(linea.base));

                    TextView descuentoPorcg = vLinea.findViewWithTag("descuento");
                    descuentoPorcg.setText(formatter.format(linea.descuentoPorc) + " %");

                    TextView ivaPorcg = vLinea.findViewWithTag("iva");
                    ivaPorcg.setText(formatter.format(linea.ivaPorc) + " %");

                    TextView totalPorcg = vLinea.findViewWithTag("total");
                    totalPorcg.setText(eurFormatter.format(linea.total));

                    break;
                }
            }
        }

    }

    private int getNLinea(List<DatosFichaLinea> lineas){
        int nLinea = 0;
        for (DatosFichaLinea linea: lineas) {
            if(linea.linea>nLinea){
                nLinea = linea.linea;
            }
        }
        return nLinea+1;
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

    private void eliminar(View v){
        try {
            DatosFichaLinea linea = (DatosFichaLinea)v.getTag();
            TableRow fila = (TableRow) v.getParent().getParent();
            int index = lineas.indexOfChild(fila);
            lineas.removeViewAt(index);
            for (int i = 0; i < ActiveData.Ficha.lineas.size(); i++) {
                if (ActiveData.Ficha.lineas.get(i).linea == linea.linea){
                    ActiveData.Ficha.lineas.remove(i);
                    break;
                }
            }
            DecimalFormat formatter = (DecimalFormat)NumberFormat.getCurrencyInstance(new Locale("es", "ES"));
            formatter.applyLocalizedPattern("#0,00");
            this.calculaTotales(formatter);
        } catch (Exception ex){
            Toast.makeText(nueva_ficha.this, "Error: " + ex.getMessage(), Toast.LENGTH_SHORT).show();
        }
    }
}