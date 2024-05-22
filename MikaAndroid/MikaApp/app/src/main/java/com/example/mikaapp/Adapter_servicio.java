package com.example.mikaapp;

import android.content.Context;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.BaseAdapter;
import android.widget.TextView;

import java.util.List;

public class Adapter_servicio extends BaseAdapter {

    Context context;
    LayoutInflater inflater;
    List<DatosServicios> lstServicios;

    public Adapter_servicio(Context appContext, List<DatosServicios> servicios) {
        this.context = appContext;
        this.lstServicios = servicios;
        inflater = (LayoutInflater.from(appContext));
    }

    @Override
    public int getCount() {
        return this.lstServicios.size();
    }

    @Override
    public Object getItem(int position) {
        DatosServicios serv = lstServicios.get(position);
        return serv;
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
        DatosServicios servicio = this.lstServicios.get(position);
        txtCodigo.setText(servicio.codigo);
        txtDescripcion.setText(servicio.nombre);
        return convertView;
    }
}
