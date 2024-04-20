package com.example.mikaapp;

import android.content.Context;
import android.content.Intent;
import android.database.Cursor;

import com.android.volley.AuthFailureError;
import com.android.volley.Request;
import com.android.volley.Response;
import com.android.volley.VolleyError;
import com.android.volley.toolbox.JsonObjectRequest;
import com.android.volley.toolbox.Volley;
import com.google.gson.Gson;

import org.json.JSONException;
import org.json.JSONObject;

import java.util.HashMap;
import java.util.Map;

public class Sincronizacion {

    private Context ctxt;
    private String token;
    private String url;

    //TODO
    //poner en el constructor tambiém el username
    //y modificar los parámetros de insertData para incluir token y username

    public Sincronizacion(String _url, String _token, Context _ctxt){
        url = _url;
        token = _token;
        ctxt = _ctxt;
    }

    public void getData(DataRequest request) throws Exception {
        try{
            JSONObject jsonObject = new JSONObject(new Gson().toJson(request));
            JsonObjectRequest jsonObjectRequest = new JsonObjectRequest(Request.Method.POST, url + "getdata", jsonObject, new Response.Listener<JSONObject>() {
                @Override
                public void onResponse(JSONObject response) {
                    DatosMika data = new Gson().fromJson(response.toString(), DatosMika.class);

                    DBManager mDbHelper = new DBManager(ctxt);
                    mDbHelper.createDatabase();
                    mDbHelper.open();

                    //Cursor testdata = mDbHelper.getTestData();
                    mDbHelper.InsertData(data);

                    mDbHelper.close();

                }
            }, new Response.ErrorListener() {
                @Override
                public void onErrorResponse(VolleyError error) {
                    try {
                        throw error;
                    } catch (VolleyError e) {
                        //throw new RuntimeException(e);
                        //TODO
                        //Manejar error de autenticación
                    }
                }
            })
                {
                @Override
                public Map<String, String> getHeaders() throws AuthFailureError {
                    Map<String, String> headers = new HashMap<>();
                    headers.put("Authorization", "Bearer " + token);
                    return headers;
                }};

            Volley.newRequestQueue(ctxt).add(jsonObjectRequest);

        }catch (Exception ex){
            throw ex;
        }
    }

}
