package com.example.mikaapp;

import android.content.Context;

/**
 * Mantiene los datos de sesi�n en memoria
 */
public class ActiveData {

    /**
     * Datos del usuario activo en la aplicaci�n
     */
    static AuthenticateResponse loginData;
    /**
     * Datos de la ficha con la que se est� trabajando
     */
    static DatosFicha Ficha;
    /**
     * Identifica si es necesaria o no una sincronizaci�n con el servidor
     */
    static boolean sincronizar;

    /**
     * Recibe los datos de autenticaci�n del usuario y los guarda en la base de datos local si procede
     * @param data datos de respuesta de la autenticaci�n del usuario
     * @param ctxt contexto del entorno de la aplicaci�n
     */
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
