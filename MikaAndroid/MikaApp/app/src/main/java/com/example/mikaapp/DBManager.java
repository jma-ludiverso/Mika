package com.example.mikaapp;

import java.io.IOException;

import android.content.ContentValues;
import android.content.Context;
import android.database.Cursor;
import android.database.SQLException;
import android.database.sqlite.SQLiteDatabase;
import android.util.Log;

public class DBManager {

    private final Context mContext;
    private SQLiteDatabase mDb;
    private DataBaseHelper mDbHelper;

    public DBManager(Context context) {
        this.mContext = context;
        mDbHelper = new DataBaseHelper(mContext);
    }

    public DBManager createDatabase() throws SQLException {
        try {
            mDbHelper.createDataBase();
        } catch (IOException mIOException) {
            //Log.e(TAG, mIOException.toString() + "  UnableToCreateDatabase");
            throw new Error("UnableToCreateDatabase");
        }
        return this;
    }

    public DBManager open() throws SQLException {
        try {
            mDbHelper.openDataBase();
            mDbHelper.close();
            mDb = mDbHelper.getReadableDatabase();
        } catch (SQLException mSQLException) {
            //Log.e(TAG, "open >>"+ mSQLException.toString());
            throw mSQLException;
        }
        return this;
    }

    public void close() {
        mDbHelper.close();
    }

    public Cursor getData(String sql, String[] args) {
        try {
            Cursor mCur = mDb.rawQuery(sql, args);
            if (mCur != null) {
                mCur.moveToNext();
            }
            return mCur;
        } catch (SQLException mSQLException) {
            throw mSQLException;
        }
    }

    public void CierraSesion(String id){
        try{
            ContentValues values = new ContentValues();
            values.put(DBStructure.USUARIOS_SECURITYSTAMP, "");
            mDb.update(DBStructure.TABLE_USUARIOS, values, DBStructure.USUARIOS_ID + "=?", new String[]{id});
            mDb.close();

        }catch (Exception ex){
            throw ex;
        }
    }

    public void InsertData(DatosMika data){
        try{

            //insertar los datos recibidos en la BD
            ContentValues values = new ContentValues();
            values.put(DBStructure.EMPRESAS_IDEMPRESA, data.datosEmpresa.idEmpresa);
            values.put(DBStructure.EMPRESAS_NOMBRE, data.datosEmpresa.nombre);
            values.put(DBStructure.EMPRESAS_CIF, data.datosEmpresa.cif);
            values.put(DBStructure.EMPRESAS_DIRECCION, data.datosEmpresa.direccion);
            values.put(DBStructure.EMPRESAS_CP, data.datosEmpresa.cp);
            values.put(DBStructure.EMPRESAS_CIUDAD, data.datosEmpresa.ciudad);
            values.put(DBStructure.EMPRESAS_TELEFONO, data.datosEmpresa.telefono);
            values.put(DBStructure.EMPRESAS_EMAIL, data.datosEmpresa.email);

            mDb.insert(DBStructure.TABLE_EMPRESAS, null, values);


        }catch (Exception ex){
            throw ex;
        }
    }

    public void InsertUser(AuthenticateResponse data){
        try{
            //insertar los datos del usuario
            ContentValues values = new ContentValues();
            values.put(DBStructure.USUARIOS_ID, data.userData.id);
            values.put(DBStructure.USUARIOS_USERNAME, data.userData.userName);
            values.put(DBStructure.USUARIOS_EMAIL, data.userData.email);
            values.put(DBStructure.USUARIOS_SECURITYSTAMP, data.token);
            values.put(DBStructure.USUARIOS_PHONENUMBER, data.userData.phoneNumber);
            values.put(DBStructure.USUARIOS_ACTIVO, data.userData.activo);
            values.put(DBStructure.USUARIOS_APELLIDOS, data.userData.apellidos);
            values.put(DBStructure.USUARIOS_NOMBRE, data.userData.nombre);
            values.put(DBStructure.USUARIOS_ISADMIN, data.userData.isAdmin);
            values.put(DBStructure.USUARIOS_CODIGO, data.userData.codigo);
            values.put(DBStructure.USUARIOS_SALON, data.userData.salon);

            long result = mDb.insert(DBStructure.TABLE_USUARIOS, null, values);
            if(result==-1){
                values = new ContentValues();
                values.put(DBStructure.USUARIOS_SECURITYSTAMP, data.token);
                mDb.update(DBStructure.TABLE_USUARIOS, values, DBStructure.USUARIOS_ID + "=?", new String[]{data.userData.id});
            }

        }catch (Exception ex){
            throw ex;
        }
    }
}
