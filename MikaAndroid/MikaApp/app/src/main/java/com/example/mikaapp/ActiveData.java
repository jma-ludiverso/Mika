package com.example.mikaapp;

import android.content.Context;

public class ActiveData {

    static AuthenticateResponse loginData;
    static DatosFicha Ficha;
    static boolean sincronizar;

    public static void setLoginData(AuthenticateResponse data, Context ctxt){
        loginData = data;
        //si sincronizar viene a true es que hay que guardar en la tabla usuarios
        if(sincronizar){
            try{
                DBManager mDbHelper = new DBManager(ctxt);
                mDbHelper.open();

                mDbHelper.InsertUser(data);

                mDbHelper.close();
            }catch (Exception ex){
                throw ex;
            }
        }
    }


}
