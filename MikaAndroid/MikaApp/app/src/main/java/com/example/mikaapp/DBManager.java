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

    public Cursor getTestData() {
        try {
            String sql ="SELECT * FROM Empresas";
            Cursor mCur = mDb.rawQuery(sql, null);
            if (mCur != null) {
                mCur.moveToNext();
            }
            return mCur;
        } catch (SQLException mSQLException) {
            //Log.e(TAG, "getTestData >>"+ mSQLException.toString());
            throw mSQLException;
        }
    }

    public void InsertData(DatosMika data){
        try{

            //TODO
            //insertar los datos recibidos en la BD

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

            mDb.insert(DBStructure.TABLE_USUARIOS, null, values);

        }catch (Exception ex){
            throw ex;
        }
    }
}
