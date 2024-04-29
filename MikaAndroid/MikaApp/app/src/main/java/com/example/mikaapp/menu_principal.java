package com.example.mikaapp;

import androidx.appcompat.app.AppCompatActivity;
import androidx.core.content.res.ResourcesCompat;

import android.content.Intent;
import android.graphics.Typeface;
import android.os.Bundle;
import android.view.View;
import android.widget.TextView;
import android.widget.Toast;

public class menu_principal extends AppCompatActivity {

    TextView txtCorreo;
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_menu_principal);

        txtCorreo = findViewById(R.id.txtCorreo);

        //Intent intent = getIntent();
        txtCorreo.setText(ActiveData.loginData.userData.userName);

        if(ActiveData.sincronizar){
            try{
                Sincronizacion sync = new Sincronizacion(getString(R.string.api_url), ActiveData.loginData.token, getApplicationContext(), ActiveData.loginData.userData.userName);
                DataRequest req = new DataRequest();
                req.salon = ActiveData.loginData.userData.salon;
                sync.getData(req);
            }catch (Exception ex){
                //Mostrar mensaje de error
                Toast.makeText(menu_principal.this, "Error: " + ex.getMessage(), Toast.LENGTH_SHORT).show();
            }
        }
    }


    public void ir_cerrar_sesion(View v){
        try{
            DBManager dbManager = new DBManager(getApplicationContext());
            dbManager.open();
            dbManager.CierraSesion(ActiveData.loginData.userData.id);
            dbManager.close();

            Toast.makeText(menu_principal.this, "Sesión cerrada correctamente", Toast.LENGTH_SHORT).show();

            // Redirigir a la actividad de inicio de sesión
            Intent intent = new Intent(menu_principal.this, MainActivity.class);
            startActivity(intent);
            finish();
        } catch (Exception ex) {
            Toast.makeText(menu_principal.this, "Error: " + ex.getMessage(), Toast.LENGTH_SHORT).show();
        }
    }

    public void ir_agregar_usuario(View v){
        Intent i = new Intent(this, agregar_usuario.class);
        startActivity(i);
    }
    public void ir_fichas_activas(View v){
        Intent i = new Intent(this, fichas_activas.class);
        startActivity(i);
    }
    public void ir_sincronizar(View v){
        Intent i = new Intent(this, sincronizar.class);
        startActivity(i);
    }

}