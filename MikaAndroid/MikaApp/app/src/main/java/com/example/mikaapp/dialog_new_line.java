package com.example.mikaapp;

import android.annotation.SuppressLint;
import android.app.Activity;
import android.app.AlertDialog;
import android.app.Dialog;
import android.content.Context;
import android.database.Cursor;
import android.graphics.Color;
import android.graphics.drawable.ColorDrawable;
import android.os.Bundle;
import android.text.Editable;
import android.text.InputType;
import android.text.TextWatcher;
import android.view.LayoutInflater;
import android.view.View;
import android.view.Window;
import android.widget.AdapterView;
import android.widget.Button;
import android.widget.EditText;
import android.widget.ImageView;
import android.widget.Spinner;
import android.widget.TextView;
import android.widget.Toast;

import androidx.fragment.app.DialogFragment;

import java.util.ArrayList;
import java.util.List;

public class dialog_new_line{
    Context dialogContext;
    TextView txtBase;
    Spinner spnEmpleado;
    EditText edtxtBaseE;
    Spinner spnServicio;
    EditText edtxtDesc;
    EditText edtxtIva;
    EditText edtxtTotal;
    String empleadoSeleccionado;
    String nombreEmpleado;
    DatosServicios servicioSelecionado;
    ImageView imgTipo;
    float catDesc = 0;

    public dialog_new_line (Context contexto, Activity act) {
        final  Dialog dialog = new Dialog(contexto);
        dialog.setOwnerActivity(act);
        dialogContext = contexto;
        dialog.requestWindowFeature(Window.FEATURE_NO_TITLE);
        dialog.setCancelable(false);
        dialog.getWindow().setBackgroundDrawable(new ColorDrawable(Color.GRAY));
        dialog.setContentView(R.layout.activity_dialog_new_line);

        final TextView txt_DialogCabecera = (TextView) dialog.findViewById(R.id.txt_DialogCabecera);
        TextView txt_Empleado = (TextView) dialog.findViewById(R.id.txt_Empleado);
        spnEmpleado = (Spinner) dialog.findViewById(R.id.spnEmpleado);
        EditText edtxt_codEmpleado = (EditText) dialog.findViewById(R.id.edtxt_codEmpleado);
        TextView txtServicio = (TextView) dialog.findViewById(R.id.txtServicio);
        spnServicio = (Spinner) dialog.findViewById(R.id.spnServicio);
        imgTipo = (ImageView) dialog.findViewById(R.id.imgTipo);
        TextView txtTipo = (TextView) dialog.findViewById(R.id.txtTipo);
        TextView txtDescripcion = (TextView) dialog.findViewById(R.id.txtDescripcion);
        EditText edtxtDescrpcion = (EditText) dialog.findViewById(R.id.edtxtDescrpcion);
        txtBase = (TextView) dialog.findViewById(R.id.txtBase);
        edtxtBaseE = (EditText) dialog.findViewById(R.id.edtxtBaseE);
        TextView txtDescuento = (TextView) dialog.findViewById(R.id.txtDescuento);
        edtxtDesc = (EditText) dialog.findViewById(R.id.edtxtDesc);
        TextView txtIva = (TextView) dialog.findViewById(R.id.txtIva);
        edtxtIva = (EditText) dialog.findViewById(R.id.edtxtIva);
        TextView txtTotal = (TextView) dialog.findViewById(R.id.txtTotal);
        edtxtTotal = (EditText) dialog.findViewById(R.id.edtxtTotal);
        Button b_cancelar = (Button) dialog.findViewById(R.id.b_cancelar);
        Button b_guardar = (Button) dialog.findViewById(R.id.b_guardar);

        this.cargaEmpleados();
        this.cargaServicios();

        spnEmpleado.setOnItemSelectedListener(new AdapterView.OnItemSelectedListener() {
            @Override
            public void onItemSelected(AdapterView<?> parent, View view, int position, long id) {
                Adapter_empleado adaptador = (Adapter_empleado) spnEmpleado.getAdapter();
                MikaWebUser emp = (MikaWebUser) adaptador.getItem(position);
                edtxt_codEmpleado.setText(emp.nombre + " " + emp.apellidos);
                empleadoSeleccionado = emp.codigo;
                nombreEmpleado = emp.nombre + " " + emp.apellidos;
            }

            @Override
            public void onNothingSelected(AdapterView<?> parent) {

            }
        });


        spnServicio.setOnItemSelectedListener(new AdapterView.OnItemSelectedListener() {
            @Override
            public void onItemSelected(AdapterView<?> parent, View view, int position, long id) {
                Adapter_servicio adaptador = (Adapter_servicio) spnServicio.getAdapter();
                DatosServicios serv = (DatosServicios) adaptador.getItem(position);
                if (serv.codigo.equals("cod.")){
                    edtxtDescrpcion.setText("");
                    edtxtBaseE.setText("");
                    edtxtDesc.setText("");
                    edtxtIva.setText("");
                    edtxtTotal.setText("");
                    imgTipo.setImageResource(android.R.color.transparent);
                    servicioSelecionado = null;
                } else {
                    edtxtDescrpcion.setText(serv.nombre);
                    edtxtBaseE.setText(String.valueOf(serv.precio));
                    edtxtDesc.setText("0");
                    edtxtIva.setText(String.valueOf(serv.ivaPorc));
                    edtxtTotal.setText(String.valueOf(serv.pvp));

                    if(serv.tipo.equals("Producto")){
                        imgTipo.setImageResource(R.drawable.p);
                    } else {
                        imgTipo.setImageResource(R.drawable.tijeras);
                    }
                    servicioSelecionado = serv;
                }

            }

            @Override
            public void onNothingSelected(AdapterView<?> parent) {

            }
        });

        b_cancelar.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                dialog.dismiss();
            }
        });

        b_guardar.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                if (!empleadoSeleccionado.equals("cod.") && servicioSelecionado!=null){
                    nueva_ficha ficha = (nueva_ficha) dialog.getOwnerActivity();
                    DatosFichaLinea linea = new DatosFichaLinea();
                    linea.base = Float.parseFloat(edtxtBaseE.getText().toString());
                    linea.codigo = empleadoSeleccionado;
                    linea.idServicio = servicioSelecionado.idServicio;
                    linea.descuentoPorc = Float.parseFloat(edtxtDesc.getText().toString());
                    linea.ivaPorc = Float.parseFloat(edtxtIva.getText().toString());
                    linea.total = Float.parseFloat(edtxtTotal.getText().toString());
                    linea.descripcion = edtxtDescrpcion.getText().toString();
                    linea.descuentoCant = catDesc;
                    linea.idSalon = ActiveData.Ficha.idSalon;
                    linea.ivaCant = servicioSelecionado.ivaCant;
                    linea.linea = 0;
                    linea.nFicha = ActiveData.Ficha.nFicha;
                    linea.nEmpleado = nombreEmpleado;
                    linea.tipo = servicioSelecionado.tipo;
                    linea.codServicio = servicioSelecionado.codigo;
                    //las comisiones se calculan en el servicio web
                    linea.comisionP1 = 0;
                    linea.comisionP2 = 0;
                    linea.comisionP3 = 0;
                    linea.comisionP4 = 0;


                    ficha.datosLinea(linea);
                    dialog.dismiss();
                } else {
                    Toast.makeText(dialogContext, "Debe seleccionar un empleado y un servicio ", Toast.LENGTH_SHORT).show();
                }

            }
        });

        edtxtBaseE.addTextChangedListener(new TextWatcher() {
            @Override
            public void beforeTextChanged(CharSequence s, int start, int count, int after) {

            }

            @Override
            public void onTextChanged(CharSequence s, int start, int before, int count) {
                CalculateDesc();
            }

            @Override
            public void afterTextChanged(Editable s) {

            }
        });

        dialog.show();
    }

    private void cargaEmpleados() {
        try {
            DBManager mDbHelper = new DBManager(dialogContext);
            mDbHelper.open();
            List<MikaWebUser> lstEmpleados = new ArrayList<MikaWebUser>();
            MikaWebUser inicial = new MikaWebUser();
            inicial.codigo = "cod.";
            inicial.nombre = "";
            inicial.apellidos = "";
            lstEmpleados.add(inicial);

            Cursor c = mDbHelper.getData("Select Codigo, Nombre, Apellidos from AspNetUsers where Salon=" + ActiveData.loginData.userData.salon + " and Activo =1 Order by Codigo", null);
            if(c != null && c.getCount()>0){
                while (c.moveToNext()){
                    MikaWebUser emp = new MikaWebUser();
                    emp.codigo = c.getString(0);
                    emp.nombre = c.getString(1);
                    emp.apellidos = c.getString(2);
                    lstEmpleados.add(emp);
                }

            }

            Adapter_empleado adaptador = new Adapter_empleado(dialogContext, lstEmpleados);
            spnEmpleado.setAdapter(adaptador);

            mDbHelper.close();
        } catch (Exception ex){
            Toast.makeText(dialogContext, "Error: " + ex.getMessage(), Toast.LENGTH_SHORT).show();
        }
    }

    @SuppressLint("Range")
    private void cargaServicios(){
        try {
            DBManager mDbHelper = new DBManager(dialogContext);
            mDbHelper.open();
            List<DatosServicios> lstServicios = new ArrayList<DatosServicios>();
            DatosServicios inicial = new DatosServicios();
            inicial.codigo = "cod.";
            inicial.nombre = "";
            lstServicios.add(inicial);

            Cursor c = mDbHelper.getData("Select * from Servicios where Activo=1 and IdEmpresa=(Select IdEmpresa from Salones where idSalon=" + ActiveData.loginData.userData.salon + ") Order by Codigo" , null);
            if(c != null && c.getCount()>0){
                while (c.moveToNext()){
                    DatosServicios serv = new DatosServicios();
                    serv.idServicio = c.getInt(c.getColumnIndex(DBStructure.SERVICIOS_ID));
                    serv.idEmpresa = c.getInt(c.getColumnIndex(DBStructure.SERVICIOS_EMPRESA));
                    serv.codigo = c.getString(c.getColumnIndex(DBStructure.SERVICIOS_CODIGO));
                    serv.tipo = c.getString(c.getColumnIndex(DBStructure.SERVICIOS_TIPO));
                    serv.grupo = c.getString(c.getColumnIndex(DBStructure.SERVICIOS_GRUPO));
                    serv.nombre = c.getString(c.getColumnIndex(DBStructure.SERVICIOS_NOMBRE));
                    serv.precio = c.getFloat(c.getColumnIndex(DBStructure.SERVICIOS_PRECIO));
                    serv.ivaPorc = c.getFloat(c.getColumnIndex(DBStructure.SERVICIOS_IVA_PORC));
                    serv.ivaCant = c.getFloat(c.getColumnIndex(DBStructure.SERVICIOS_IVA_CANT));
                    serv.pvp = c.getFloat(c.getColumnIndex(DBStructure.SERVICIOS_PVP));
                    serv.activo = Boolean.parseBoolean(c.getString(c.getColumnIndex(DBStructure.SERVICIOS_ACTIVO)));
                    lstServicios.add(serv);
                }

            }

            Adapter_servicio adaptador = new Adapter_servicio(dialogContext, lstServicios);
            spnServicio.setAdapter(adaptador);

            mDbHelper.close();

        } catch (Exception ex){
            Toast.makeText(dialogContext, "Error: " + ex.getMessage(), Toast.LENGTH_SHORT).show();
        }
    }

    private void CalculateDesc()
    {
        //debe saltar si cambia el precio base o el porcentaje de descuento
        if (edtxtDesc.getText().equals(""))
        {
            edtxtDesc.setText("0");
        }
        if (edtxtBaseE.getText().equals(""))
        {
            edtxtBaseE.setText("0");
        }
        float desc = Float.parseFloat(edtxtDesc.getText().toString());
        float precio = Float.parseFloat(edtxtBaseE.getText().toString());
        float cantidad = precio * (desc / 100);
        catDesc = cantidad;
        this.CalculateIva();
        this.CalculatePVP();
}

    private void CalculateIva()
    {
        //debe saltar si cambia el porcentaje de iva
        //(hay que ajustar los nombres de los editText y de la variable servicio seleccionado a los correctos)
        if (edtxtBaseE.getText().equals(""))
        {
            edtxtBaseE.setText("0");
        }
        if (edtxtIva.getText().equals(""))
        {
            edtxtIva.setText("0");
        }
        float precio = Float.parseFloat(edtxtBaseE.getText().toString());
        float iva = Float.parseFloat(edtxtIva.getText().toString());
        float cantidad = (precio - catDesc) * (iva / 100);
        servicioSelecionado.ivaCant = cantidad;
        this.CalculatePVP();
    }

    private void CalculatePVP()
    {
        //debe saltar si cambia el precio total
        //(hay que ajustar los nombres de los editText y de la variable servicio seleccionado a los correctos)
        if (edtxtTotal.getText().equals(""))
        {
            edtxtTotal.setText("0");
        }
        if (edtxtIva.getText().equals(""))
        {
            edtxtIva.setText("0");
        }
        float precio = Float.parseFloat(edtxtTotal.getText().toString());
        float iva = Float.parseFloat(edtxtIva.getText().toString());
        float preciobase = precio / ((iva / 100) + 1);
        edtxtBaseE.setText(String.valueOf(preciobase));
        edtxtDesc.setText("0");
        if (servicioSelecionado != null){
            servicioSelecionado.ivaCant = precio - preciobase;
        }
        catDesc = 0;
    }
}