package com.example.mikaapp;

import androidx.annotation.StringRes;
import androidx.appcompat.app.AppCompatActivity;

import android.annotation.SuppressLint;
import android.app.DownloadManager;
import android.content.Intent;
import android.database.Cursor;
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
import java.text.SimpleDateFormat;
import java.time.LocalDate;
import java.util.Date;
import java.util.HashMap;
import java.util.Locale;
import java.util.Map;
import java.util.TimeZone;

public class MainActivity extends AppCompatActivity {
    EditText userName,password;
    Button btnIniciar;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

        //comprobar si tenemos un usuario logado (token existente)
        //si hay token recuperamos los datos y llamamos al menú principal llenando la clase ActiveData con los datos del usuario y no sincronizamos
        //si no hay token permanecemos en esta vista para hacer login, sí sincronizamos

        if(this.DatosUsuario()){
            //Antes de iniciar la actividad se verifica si el token está próximo a expirar (menos de 2 días)
            Date actual = new Date();
            long time_difference = ActiveData.loginData.tokenExpires.getTime() - actual.getTime();
            long dias_diference = (time_difference/(1000*60*60*24)) % 365;
            if (dias_diference <= 2){
                this.TokenRenovacion();
            }
            else{
                // Crear un Intent para iniciar la actividad del menú principal
                Intent intent = new Intent(MainActivity.this, menu_principal.class);
                startActivity(intent);
                finish();
            }
        }else{
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
        }
    }

    @SuppressLint("Range")
    private boolean DatosUsuario(){
        boolean ret = false;
        try{
            DBManager mDbHelper = new DBManager(getApplicationContext());
            mDbHelper.createDatabase();
            mDbHelper.open();

            Cursor c = mDbHelper.getData("Select * from AspnetUsers where SecurityStamp<>''", null);
            if(c != null && c.getCount()>0){  //si entra en este if, tenemos datos de usuario logado
                c.moveToFirst();
                ActiveData.sincronizar = false;
                AuthenticateResponse data = new AuthenticateResponse();
                data.userData = new MikaWebUser();
                data.token = c.getString(c.getColumnIndex(DBStructure.USUARIOS_SECURITYSTAMP));
                SimpleDateFormat formatter = new SimpleDateFormat("EEE MMM dd hh:mm:ss zzz yyyy", Locale.ENGLISH);
                formatter.setTimeZone(TimeZone.getTimeZone("Europe/Madrid"));
                data.tokenExpires = formatter.parse(c.getString(c.getColumnIndex(DBStructure.USUARIOS_SECURITYEXPIRATION)));
                Date actual = new Date();

                //Se pregunta si el token ha expirado
                if(actual.before(data.tokenExpires)){
                    data.userData.id = c.getString(c.getColumnIndex(DBStructure.USUARIOS_ID));
                    data.userData.userName = c.getString(c.getColumnIndex(DBStructure.USUARIOS_USERNAME));
                    data.userData.email = c.getString(c.getColumnIndex(DBStructure.USUARIOS_EMAIL));
                    data.userData.phoneNumber = c.getString(c.getColumnIndex(DBStructure.USUARIOS_PHONENUMBER));
                    data.userData.activo = Boolean.parseBoolean(c.getString(c.getColumnIndex(DBStructure.USUARIOS_ACTIVO)));
                    data.userData.apellidos = c.getString(c.getColumnIndex(DBStructure.USUARIOS_APELLIDOS));
                    data.userData.nombre = c.getString(c.getColumnIndex(DBStructure.USUARIOS_NOMBRE));
                    data.userData.isAdmin = Boolean.parseBoolean(c.getString(c.getColumnIndex(DBStructure.USUARIOS_ISADMIN)));
                    data.userData.codigo = c.getString(c.getColumnIndex(DBStructure.USUARIOS_CODIGO));
                    data.userData.salon = c.getInt(c.getColumnIndex(DBStructure.USUARIOS_SALON));
                    ActiveData.setLoginData(data, getApplicationContext());
                    ret = true;
                }

            }
                mDbHelper.close();

        }catch (Exception ex){
            // Mostrar un Toast indicando que se ha producido un error
            Toast.makeText(MainActivity.this, "Error: " + ex.getMessage(), Toast.LENGTH_SHORT).show();
            ret = false;
        }
        return ret;
    }

    private void InicioSesion() {
        try{
            //TODO: Controlar que si no se han puesto usuario y contraseña que no haga nada

            String url = getString(R.string.api_url);
            url += "authenticate";

            AuthenticateRequest request = new AuthenticateRequest();
            request.userName = String.valueOf(userName.getText());
            request.password = String.valueOf(password.getText());

            JSONObject jsonObject = new JSONObject(new Gson().toJson(request));

            JsonObjectRequest jsonObjectRequest = new JsonObjectRequest(Request.Method.POST, url, jsonObject, new Response.Listener<JSONObject>() {
                public void onResponse(JSONObject response) {
                    MainActivity.this.AutenticacionCorrecta(response);
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

    public void AutenticacionCorrecta(JSONObject response){
        AuthenticateResponse resp = new Gson().fromJson(response.toString(), AuthenticateResponse.class);
        if (resp.succeeded) {
            ActiveData.sincronizar = true;
            ActiveData.setLoginData(resp, getApplicationContext());
            // Crear un Intent para iniciar la actividad del menú principal
            Intent intent = new Intent(MainActivity.this, menu_principal.class);
            startActivity(intent);
            finish();
        } else {
            // Manejar el caso en que el inicio de sesión no sea exitoso
            // Mostrar un Toast indicando que el inicio de sesión no fue exitoso
            Toast.makeText(MainActivity.this, "Inicio de sesión fallido", Toast.LENGTH_SHORT).show();
        }
    }

    private void TokenRenovacion() {

        try {
            String url = getString(R.string.api_url);
            url += "authenticaterenewal";

            JsonObjectRequest jsonObjectRequest = new JsonObjectRequest(Request.Method.POST, url, null, new Response.Listener<JSONObject>() {
                @Override
                public void onResponse(JSONObject response) {
                    MainActivity.this.AutenticacionCorrecta(response);
                }
            }, new Response.ErrorListener() {
                @Override
                public void onErrorResponse(VolleyError error) {
                    try {
                        throw error;
                    } catch (VolleyError e) {
                        //throw new RuntimeException(e);

                        //Manejar error de autenticación
                        Toast.makeText(getApplicationContext(), "Error de renovación. Por favor, inicia sesión nuevamente.", Toast.LENGTH_LONG).show();
                        // Redirigir al usuario a la pantalla de inicio de sesión
                        Intent intent = new Intent(getApplicationContext(), MainActivity.class);
                        intent.addFlags(Intent.FLAG_ACTIVITY_CLEAR_TOP | Intent.FLAG_ACTIVITY_NEW_TASK);
                        getApplicationContext().startActivity(intent);
                    }
                }
            })
            {
                @Override
                public Map<String, String> getHeaders() throws AuthFailureError {
                    Map<String, String> headers = new HashMap<>();
                    headers.put("Authorization", "Bearer " + ActiveData.loginData.token);
                    return headers;
                }};

            Volley.newRequestQueue(getApplicationContext()).add(jsonObjectRequest);
        } catch (Exception ex){
            // Mostrar un Toast indicando que se ha producido un error
            Toast.makeText(MainActivity.this, "Error: " + ex.getMessage(), Toast.LENGTH_SHORT).show();
        }
    }



}