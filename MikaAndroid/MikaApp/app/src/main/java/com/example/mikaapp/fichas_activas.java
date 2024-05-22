package com.example.mikaapp;

import androidx.appcompat.app.AppCompatActivity;

import android.app.DatePickerDialog;
import android.content.Intent;
import android.os.Bundle;
import android.view.View;
import android.view.WindowManager;
import android.widget.DatePicker;
import android.widget.TextView;

import java.text.SimpleDateFormat;
import java.util.Calendar;
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
                String fecha = dayOfMonth +"/"+ month +"/"+ year;
                date.setText(fecha);
            }
        }, anio, mes, dia);
        dpd.show();
    }


}