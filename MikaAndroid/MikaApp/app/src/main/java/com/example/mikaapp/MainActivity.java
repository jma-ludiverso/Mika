package com.example.mikaapp;

import androidx.annotation.StringRes;
import androidx.appcompat.app.AppCompatActivity;

import android.app.DownloadManager;
import android.content.Intent;
import android.os.Bundle;
import android.text.InputType;
import android.util.Log;
import android.view.View;
import android.widget.Button;
import android.widget.CheckBox;
import android.widget.CompoundButton;
import android.widget.EditText;
import android.widget.Toast;

import com.android.volley.AuthFailureError;
import com.android.volley.Request;
import com.android.volley.Response;
import com.android.volley.VolleyError;
import com.android.volley.toolbox.JsonObjectRequest;
import com.android.volley.toolbox.StringRequest;
import com.android.volley.toolbox.Volley;
import com.google.gson.Gson;

import org.json.JSONException;
import org.json.JSONObject;

import java.net.ResponseCache;
import java.util.HashMap;
import java.util.Map;

public class MainActivity extends AppCompatActivity {
    EditText userName,password;
    Button btnIniciar;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

        userName = findViewById(R.id.txtEmail);
        password = findViewById(R.id.txtCotrasena);
        btnIniciar = findViewById(R.id.b_IniciarSesion);

        btnIniciar.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                InicioSesion();
            }
        });



        CheckBox checkBox = findViewById(R.id.boxContrasena);
        EditText editTextPassword = findViewById(R.id.txtCotrasena);
        checkBox.setOnCheckedChangeListener(new CompoundButton.OnCheckedChangeListener() {
            @Override
            public void onCheckedChanged(CompoundButton buttonView, boolean isChecked) {
                if (isChecked) {
                    // Si el checkbox está marcado, mostrar la contraseña
                    editTextPassword.setInputType(InputType.TYPE_TEXT_VARIATION_VISIBLE_PASSWORD);
                } else {
                    // Si el checkbox no está marcado, ocultar la contraseña
                    editTextPassword.setInputType(InputType.TYPE_CLASS_TEXT | InputType.TYPE_TEXT_VARIATION_PASSWORD);
                }
            }
        });

        //TODO
        //comprobar si tenemos un usuario logado (token existente)
        //si hay token recuperamos los datos y llamamos al menú principal llenando la clase ActiveData con los datos del usuario y no sincronizamos
        //si no hay token permanecemos en esta vista para hacer login, sí sincronizamos

    }

    private void InicioSesion() {
        try{
            String url = getString(R.string.api_url);
            url += "authenticate";

            AuthenticateRequest request = new AuthenticateRequest();
            request.userName = String.valueOf(userName.getText());
            request.password = String.valueOf(password.getText());

            JSONObject jsonObject = new JSONObject(new Gson().toJson(request));

            JsonObjectRequest jsonObjectRequest = new JsonObjectRequest(Request.Method.POST, url, jsonObject, new Response.Listener<JSONObject>() {
                public void onResponse(JSONObject response) {
                    AuthenticateResponse resp = new Gson().fromJson(response.toString(), AuthenticateResponse.class);
                    if (resp.succeeded) {
                        ActiveData.setLoginData(resp);
                        ActiveData.sincronizar = true;
                        // Crear un Intent para iniciar la actividad del menú principal
                        Intent intent = new Intent(MainActivity.this, menu_principal.class);
                        //intent.putExtra("loginData", resp);
                        startActivity(intent);
                        finish();
                    } else {
                        // Manejar el caso en que el inicio de sesión no sea exitoso
                        // Mostrar un Toast indicando que el inicio de sesión no fue exitoso
                        Toast.makeText(MainActivity.this, "Inicio de sesión fallido", Toast.LENGTH_SHORT).show();
                    }

                }
            }, new Response.ErrorListener() {
                @Override
                public void onErrorResponse(VolleyError error) {
                    // Mostrar un Toast indicando que se ha producido un error
                    Toast.makeText(MainActivity.this, "Error: " + error.getMessage(), Toast.LENGTH_SHORT).show();
                }
            });

            Volley.newRequestQueue(this).add(jsonObjectRequest);

        }catch (Exception ex){
            // Mostrar un Toast indicando que se ha producido un error
            Toast.makeText(MainActivity.this, "Error: " + ex.getMessage(), Toast.LENGTH_SHORT).show();
        }


    }

}