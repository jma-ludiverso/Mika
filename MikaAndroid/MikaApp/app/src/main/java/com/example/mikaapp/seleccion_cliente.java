package com.example.mikaapp;

import static android.app.ProgressDialog.show;

import androidx.appcompat.app.AppCompatActivity;

import android.content.Intent;
import android.database.Cursor;
import android.os.Bundle;
import android.view.KeyEvent;
import android.view.View;
import android.view.WindowManager;
import android.widget.AdapterView;
import android.widget.Button;
import android.widget.EditText;
import android.widget.ListView;
import android.widget.TextView;
import android.widget.Toast;

import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Calendar;
import java.util.Date;
import java.util.List;
import java.util.Locale;

public class seleccion_cliente extends AppCompatActivity {
    TextView txtCorreo;
    ListView lvClientes;
    EditText buscador;
    EditText nombreCliente;
    EditText telefonoCliente;
    EditText emailCliente;
    Button bAgregarCliente;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_seleccion_cliente);

        txtCorreo = findViewById(R.id.txtCorreo);
        //Intent intent = getIntent();
        txtCorreo.setText(ActiveData.loginData.userData.userName);

        nombreCliente = findViewById(R.id.editTextNombre);
        telefonoCliente = findViewById(R.id.editTextTelefono);
        emailCliente = findViewById(R.id.editTextSEmail);
        bAgregarCliente = findViewById(R.id.b_AgregarCliente);

        getWindow().setSoftInputMode(WindowManager.LayoutParams.SOFT_INPUT_ADJUST_PAN);

        lvClientes = findViewById(R.id.lista_de_clientes);
        List<DatosCliente> lstClientes = new ArrayList<DatosCliente>();
        DatosCliente cli = new DatosCliente();
        cli.idCliente = 0;
        cli.nombre = "Cliente genérico";
        lstClientes.add(cli);
        Adapter_cliente adaptador = new Adapter_cliente(getApplicationContext(), lstClientes);
        lvClientes.setAdapter(adaptador);

        lvClientes.setOnItemClickListener(new AdapterView.OnItemClickListener() {
            @Override
            public void onItemClick(AdapterView<?> parent, View view, int position, long id) {
                Adapter_cliente adaptador = (Adapter_cliente) lvClientes.getAdapter();
                DatosCliente cli = (DatosCliente) adaptador.getItem(position);
                seleccionCliente(cli.idCliente,cli.nombre);
            }
        });

        buscador = findViewById(R.id.lupaCliente);
        buscador.setOnKeyListener(new EditText.OnKeyListener() {
            @Override
            public boolean onKey(View v, int keyCode, KeyEvent event) {
                EditText txt = (EditText) v;
                buscarCliente(String.valueOf(txt.getText()));
                return false;
            }
        });

        bAgregarCliente.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                nuevoCliente();
            }
        });
    }

    public void ir_nueva_ficha(View v){
        Intent i = new Intent(this, nueva_ficha.class);
        startActivity(i);
    }

    public void seleccionCliente(int idCliente, String nombreCliente){
        try{
            Intent i = new Intent(this, nueva_ficha.class);
            ActiveData.Ficha = new DatosFicha();
            ActiveData.Ficha.idCliente = idCliente;
            ActiveData.Ficha.nombreCliente = nombreCliente;
            ActiveData.Ficha.idSalon = ActiveData.loginData.userData.salon;
            SimpleDateFormat dateFormat = new SimpleDateFormat("dd/MM/yyyy", Locale.getDefault());
            Calendar cal = Calendar.getInstance();
            ActiveData.Ficha.fecha = dateFormat.format(cal.getTime());
            ActiveData.Ficha.anio = cal.get(Calendar.YEAR);
            ActiveData.Ficha.mes = cal.get(Calendar.MONTH);
            ActiveData.Ficha.formaPago = "Tarjeta";
            ActiveData.Ficha.descuentos = 0;
            ActiveData.Ficha.descuentoPorc = 0;
            ActiveData.Ficha.descuentoImp = 0;
            ActiveData.Ficha.iva = 0;
            ActiveData.Ficha.total = 0;
            ActiveData.Ficha.pagado = 0;
            ActiveData.Ficha.cambio = 0;
            ActiveData.Ficha.lineas = new ArrayList<>();
            ActiveData.Ficha.numero = this.numeroFicha(ActiveData.Ficha.anio, ActiveData.Ficha.mes);
            ActiveData.Ficha.nFicha = String.valueOf(ActiveData.Ficha.anio) + String.format("%02d", ActiveData.Ficha.mes) + String.format("%03d", ActiveData.Ficha.numero);
            startActivity(i);

        }catch (Exception ex){
            Toast.makeText(seleccion_cliente.this, "Error: " + ex.getMessage(), Toast.LENGTH_SHORT).show();
        }
    }

    public void buscarCliente(String buscando){
        try {
            DBManager mDbHelper = new DBManager(getApplicationContext());
            mDbHelper.open();
            List<DatosCliente> lstClientes = new ArrayList<DatosCliente>();

            Cursor c = mDbHelper.getData("Select idCliente, Nombre from Clientes where idSalon=" + ActiveData.loginData.userData.salon + " and Nombre like '%" + buscando + "%'", null);
            if(c != null && c.getCount()>0){
                while (c.moveToNext()){
                    DatosCliente cli = new DatosCliente();
                    cli.idCliente = c.getInt(0);
                    cli.nombre = c.getString(1);
                    lstClientes.add(cli);
                }

            }

            Adapter_cliente adaptador = new Adapter_cliente(getApplicationContext(), lstClientes);
            lvClientes.setAdapter(adaptador);

            mDbHelper.close();

        } catch (Exception ex){
            Toast.makeText(seleccion_cliente.this, "Error: " + ex.getMessage(), Toast.LENGTH_SHORT).show();
        }
    }

    public void nuevoCliente(){
        try {
            DBManager mDbHelper = new DBManager(getApplicationContext());
            mDbHelper.open();

            List<DatosCliente> listaClientes = new ArrayList<>();

            DatosCliente nuevoCliente = new DatosCliente();
            Cursor c = mDbHelper.getData("Select max (idCliente) +1 from Clientes", null);
            c.moveToFirst();
            nuevoCliente.idCliente = c.getInt(0);
            nuevoCliente.idSalon = ActiveData.loginData.userData.salon;
            nuevoCliente.nombre = nombreCliente.getText().toString();
            nuevoCliente.telefono = telefonoCliente.getText().toString();
            nuevoCliente.email = emailCliente.getText().toString();
            nuevoCliente.nuevo = true;
            nuevoCliente.historial = new ArrayList<>();
            listaClientes.add(nuevoCliente);

            mDbHelper.insertClientes(listaClientes);

            mDbHelper.close();

            this.seleccionCliente(nuevoCliente.idCliente, nuevoCliente.nombre);

        } catch (Exception ex){
            Toast.makeText(seleccion_cliente.this, "Error: " + ex.getMessage(), Toast.LENGTH_SHORT).show();
        }
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
}