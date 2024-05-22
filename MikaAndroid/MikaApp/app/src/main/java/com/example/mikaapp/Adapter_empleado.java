package com.example.mikaapp;

import android.content.Context;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.BaseAdapter;
import android.widget.ImageView;
import android.widget.TextView;

import java.util.List;

public class Adapter_empleado extends BaseAdapter {

    Context context;
    LayoutInflater inflater;
    List<MikaWebUser> lstEmpleados;

    public Adapter_empleado(Context appContext, List<MikaWebUser> empleados) {
        this.context = appContext;
        this.lstEmpleados = empleados;
        inflater = (LayoutInflater.from(appContext));
    }

    @Override
    public int getCount() {
        return this.lstEmpleados.size();
    }

    @Override
    public Object getItem(int position) {
        MikaWebUser emp = lstEmpleados.get(position);
        return emp;
    }

    @Override
    public long getItemId(int position) {
        return 0;
    }

    @Override
    public View getView(int position, View convertView, ViewGroup parent) {
        convertView = inflater.inflate(R.layout.fila_spinner, null);
        TextView txtCodigo = (TextView)convertView.findViewById(R.id.spinCodigo);
        TextView txtDescripcion = (TextView)convertView.findViewById(R.id.SpinDescripcion);
        MikaWebUser empleado = this.lstEmpleados.get(position);
        txtCodigo.setText(empleado.codigo);
        txtDescripcion.setText(empleado.nombre + " " + empleado.apellidos);
        return convertView;
    }
}
