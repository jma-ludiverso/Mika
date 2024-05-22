package com.example.mikaapp;

import android.content.Context;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.BaseAdapter;
import android.widget.ImageView;
import android.widget.TextView;

import com.example.mikaapp.DatosCliente;
import com.example.mikaapp.R;

import java.util.List;

public class Adapter_cliente extends BaseAdapter {

    Context context;
    LayoutInflater inflater;
    List<DatosCliente> lstClientes;

    public Adapter_cliente(Context appContext, List<DatosCliente> clientes) {
        this.context = appContext;
        this.lstClientes = clientes;
        inflater = (LayoutInflater.from(appContext));
    }

    @Override
    public int getCount() {
        return this.lstClientes.size();
    }

    @Override
    public Object getItem(int position) {
        DatosCliente cli = lstClientes.get(position);
        return cli;
    }

    @Override
    public long getItemId(int position) {
        return 0;
    }

    @Override
    public View getView(int position, View convertView, ViewGroup parent) {
        convertView = inflater.inflate(R.layout.fila_clente, null);
        TextView txtCliente = (TextView)convertView.findViewById(R.id.txt_NombreCliente);
        ImageView img = convertView.findViewById(R.id.imgFilaCliente);
        img.setImageResource(R.drawable.person);
        DatosCliente cliente = this.lstClientes.get(position);
        txtCliente.setText(cliente.nombre);
        return convertView;
    }
}
