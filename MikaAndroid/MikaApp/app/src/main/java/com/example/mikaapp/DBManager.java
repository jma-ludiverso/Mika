package com.example.mikaapp;

import java.io.IOException;
import java.util.List;

import android.content.ContentValues;
import android.content.Context;
import android.database.Cursor;
import android.database.SQLException;
import android.database.sqlite.SQLiteDatabase;

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
           /* if (mCur != null) {
                mCur.moveToNext();
            }  */
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

            insertSalon(data.datosSalon);
            insertClientes(data.listaClientes);
            insertServicio(data.listaServicios);

        }catch (Exception ex){
            throw ex;
        }
    }

    public void insertClientes(List<DatosCliente> data) {
        try {
            for (DatosCliente cli: data)
            {
                ContentValues values = new ContentValues();
                values.put(DBStructure.CLIENTES_ID, cli.idCliente);
                values.put(DBStructure.CLIENTES_SALON, cli.idSalon);
                values.put(DBStructure.CLIENTES_NOMBRE, cli.nombre);
                values.put(DBStructure.CLIENTES_TELEFONO, cli.telefono);
                values.put(DBStructure.CLIENTES_EMAIL, cli.email);
                values.put(DBStructure.CLIENTES_NUEVO, cli.nuevo);

                // Insertar los datos del cliente en la base de datos
                mDb.insert(DBStructure.TABLE_CLIENTES, null, values);

                for (DatosClientesHistorial hist : cli.historial) {
                    values = new ContentValues();
                    values.put(DBStructure.CLIENTESHISTORIA_IDCLIENTE, cli.idCliente);
                    values.put(DBStructure.CLIENTESHISTORIA_IDHISTORIA, hist.idHistoria);
                    values.put(DBStructure.CLIENTESHISTORIA_FECHA, hist.fecha);
                    values.put(DBStructure.CLIENTESHISTORIA_DESCRIPCION, hist.descripcion);
                    values.put(DBStructure.CLIENTESHISTORIA_NUEVA, hist.nueva);

                    mDb.insert(DBStructure.TABLE_CLIENTESHISTORIAL, null, values);
                }

            }
        } catch (Exception ex) {
            throw ex;
        }
    }


    private void insertEmpleadoComisiones(List<DatosEmpleadosComisiones> data) {
        try {
            for (DatosEmpleadosComisiones empco : data ) {

                ContentValues values = new ContentValues();
                values.put(DBStructure.EMPLEADOSCOMISIONES_CODIGO, empco.codigo);
                values.put(DBStructure.EMPLEADOSCOMISIONES_SALON, empco.idSalon);
                values.put(DBStructure.EMPLEADOSCOMISIONES_PRODUCTOS_E1, empco.productoE1);
                values.put(DBStructure.EMPLEADOSCOMISIONES_PRODUCTOS_E2, empco.productoE2);
                values.put(DBStructure.EMPLEADOSCOMISIONES_PRODUCTOS_E3, empco.productoE3);
                values.put(DBStructure.EMPLEADOSCOMISIONES_PRODUCTOS_E4, empco.productoE4);
                values.put(DBStructure.EMPLEADOSCOMISIONES_PRODUCTOS_P1, empco.productoP1);
                values.put(DBStructure.EMPLEADOSCOMISIONES_PRODUCTOS_P2, empco.productoP2);
                values.put(DBStructure.EMPLEADOSCOMISIONES_PRODUCTOS_P3, empco.productoP3);
                values.put(DBStructure.EMPLEADOSCOMISIONES_PRODUCTOS_P4, empco.productoP4);
                values.put(DBStructure.EMPLEADOSCOMISIONES_SERVICIOS_LE1, empco.servicioLE1);
                values.put(DBStructure.EMPLEADOSCOMISIONES_SERVICIOS_LE2, empco.servicioLE2);
                values.put(DBStructure.EMPLEADOSCOMISIONES_SERVICIOS_LE3, empco.servicioLE3);
                values.put(DBStructure.EMPLEADOSCOMISIONES_SERVICIOS_LE4, empco.servicioLE4);
                values.put(DBStructure.EMPLEADOSCOMISIONES_SERVICIOS_LP1, empco.servicioLP1);
                values.put(DBStructure.EMPLEADOSCOMISIONES_SERVICIOS_LP2, empco.servicioLP2);
                values.put(DBStructure.EMPLEADOSCOMISIONES_SERVICIOS_LP3, empco.servicioLP3);
                values.put(DBStructure.EMPLEADOSCOMISIONES_SERVICIOS_LP4, empco.servicioLP4);
                values.put(DBStructure.EMPLEADOSCOMISIONES_SERVICIOS_SE1, empco.servicioSE1);
                values.put(DBStructure.EMPLEADOSCOMISIONES_SERVICIOS_SE2, empco.servicioSE2);
                values.put(DBStructure.EMPLEADOSCOMISIONES_SERVICIOS_SE3, empco.servicioSE3);
                values.put(DBStructure.EMPLEADOSCOMISIONES_SERVICIOS_SE4, empco.servicioSE4);
                values.put(DBStructure.EMPLEADOSCOMISIONES_SERVICIOS_SP1, empco.servicioSP1);
                values.put(DBStructure.EMPLEADOSCOMISIONES_SERVICIOS_SP2, empco.servicioSP2);
                values.put(DBStructure.EMPLEADOSCOMISIONES_SERVICIOS_SP3, empco.servicioSP3);
                values.put(DBStructure.EMPLEADOSCOMISIONES_SERVICIOS_SP4, empco.servicioSP4);
                values.put(DBStructure.EMPLEADOSCOMISIONES_SERVICIOS_TE1, empco.servicioTE1);
                values.put(DBStructure.EMPLEADOSCOMISIONES_SERVICIOS_TE2, empco.servicioTE2);
                values.put(DBStructure.EMPLEADOSCOMISIONES_SERVICIOS_TE3, empco.servicioTE3);
                values.put(DBStructure.EMPLEADOSCOMISIONES_SERVICIOS_TE4, empco.servicioTE4);
                values.put(DBStructure.EMPLEADOSCOMISIONES_SERVICIOS_TP1, empco.servicioTP1);
                values.put(DBStructure.EMPLEADOSCOMISIONES_SERVICIOS_TP2, empco.servicioTP2);
                values.put(DBStructure.EMPLEADOSCOMISIONES_SERVICIOS_TP3, empco.servicioTP3);
                values.put(DBStructure.EMPLEADOSCOMISIONES_SERVICIOS_TP4, empco.servicioTP4);

                // Insertar los datos del empleado en la base de datos
                mDb.insert(DBStructure.TABLE_EMPLEADOSCOMISIONES, null, values);

                insertEmpleadoServicios(empco.comisiones, empco.codigo);

            }

        } catch (Exception ex) {
            throw ex;
        }
    }

    private void insertServicio(List<DatosServicios> data) {
        try {
            for (DatosServicios ser : data) {

                ContentValues values = new ContentValues();
                values.put(DBStructure.SERVICIOS_ID, ser.idServicio);
                values.put(DBStructure.SERVICIOS_EMPRESA, ser.idEmpresa);
                values.put(DBStructure.SERVICIOS_CODIGO, ser.codigo);
                values.put(DBStructure.SERVICIOS_TIPO, ser.tipo);
                values.put(DBStructure.SERVICIOS_GRUPO, ser.grupo);
                values.put(DBStructure.SERVICIOS_NOMBRE, ser.nombre);
                values.put(DBStructure.SERVICIOS_PRECIO, ser.precio);
                values.put(DBStructure.SERVICIOS_IVA_PORC, ser.ivaPorc);
                values.put(DBStructure.SERVICIOS_IVA_CANT, ser.ivaCant);
                values.put(DBStructure.SERVICIOS_PVP, ser.pvp);
                values.put(DBStructure.SERVICIOS_ACTIVO, ser.activo);

                // Insertar los datos del servicio en la base de datos
                mDb.insert(DBStructure.TABLE_SERVICIOS, null, values);
            }

        } catch (Exception ex) {
            throw ex;
        }
    }

    private void insertEmpleadoServicios(List<DatosEmpleadosServicios> data, String codigo) {

        try {
            for (DatosEmpleadosServicios empse : data) {

                ContentValues values = new ContentValues();
                values.put(DBStructure.EMPLEADOSSERVICIOS_CODIGO, codigo);
                values.put(DBStructure.EMPLEADOSSERVICIOS_ID_SERVICIO, empse.idServicio);
                values.put(DBStructure.EMPLEADOSSERVICIOS_COMISION_P1, empse.comisionP1);
                values.put(DBStructure.EMPLEADOSSERVICIOS_COMISION_P2, empse.comisionP2);
                values.put(DBStructure.EMPLEADOSSERVICIOS_COMISION_P3, empse.comisionP3);
                values.put(DBStructure.EMPLEADOSSERVICIOS_COMISION_P4, empse.comisionP4);

                // Insertar los datos del empleado en la tabla de servicios
                mDb.insert(DBStructure.TABLE_EMPLEADOSSERVICIOS, null, values);
            }

        } catch (Exception ex) {
            throw ex;
        }
    }

    private void insertSalon(DatosSalon data) {
        try {
            ContentValues values = new ContentValues();
            values.put(DBStructure.SALONES_ID, data.id);
            values.put(DBStructure.SALONES_EMPRESA, data.idEmpresa);
            values.put(DBStructure.SALONES_NOMBRE, data.nombre);
            values.put(DBStructure.SALONES_DIRECCION, data.direccion);
            values.put(DBStructure.SALONES_TELEFONO, data.telefono);

            // Insertar los datos del salón en la base de datos
            mDb.insert(DBStructure.TABLE_SALONES, null, values);

            for (MikaWebUser user : data.empleados) {
                AuthenticateResponse ar = new AuthenticateResponse();
                ar.userData = user;
                InsertUser(ar);
            }

            insertEmpleadoComisiones(data.empleadosComisiones);

        } catch (Exception ex) {
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
            if (data.tokenExpires == null){
                values.put(DBStructure.USUARIOS_SECURITYEXPIRATION, "");
            }else {
                values.put(DBStructure.USUARIOS_SECURITYEXPIRATION, data.tokenExpires.toString());
            }

            long result = mDb.insert(DBStructure.TABLE_USUARIOS, null, values);
            if(result==-1){
                values = new ContentValues();
                values.put(DBStructure.USUARIOS_SECURITYSTAMP, data.token);
                if (data.tokenExpires == null){
                    values.put(DBStructure.USUARIOS_SECURITYEXPIRATION, "");
                }else {
                    values.put(DBStructure.USUARIOS_SECURITYEXPIRATION, data.tokenExpires.toString());
                }

                mDb.update(DBStructure.TABLE_USUARIOS, values, DBStructure.USUARIOS_ID + "=?", new String[]{data.userData.id});
            }

        }catch (Exception ex){
            throw ex;
        }
    }

    public void insertFichas(List<DatosFicha> data) {
        try {
            for (DatosFicha ficha : data) {

                ContentValues values = new ContentValues();
                values.put(DBStructure.FICHAS_NFICHA, ficha.nFicha);
                values.put(DBStructure.FICHAS_FECHA, ficha.fecha);
                values.put(DBStructure.FICHAS_ANIO, ficha.anio);
                values.put(DBStructure.FICHAS_MES, ficha.mes);
                values.put(DBStructure.FICHAS_NUMERO, ficha.numero);
                values.put(DBStructure.FICHAS_IDSALON, ficha.idSalon);
                values.put(DBStructure.FICHAS_IDCLIENTE, ficha.idCliente);
                values.put(DBStructure.FICHAS_FORMAPAGO, ficha.formaPago);
                values.put(DBStructure.FICHAS_BASE, ficha.base);
                values.put(DBStructure.FICHAS_DESCUENTOPORC, ficha.descuentoPorc);
                values.put(DBStructure.FICHAS_DESCUENTOIMP, ficha.descuentoImp);
                values.put(DBStructure.FICHAS_DESCUENTOS, ficha.descuentos);
                values.put(DBStructure.FICHAS_ILVA, ficha.iva);
                values.put(DBStructure.FICHAS_TOTAL, ficha.total);
                values.put(DBStructure.FICHAS_PAGADO, ficha.pagado);
                values.put(DBStructure.FICHAS_CAMBIO, ficha.cambio);
                values.put(DBStructure.FICHAS_CERRADA, ficha.cerrada);

                if (ficha.nFichaAnterior.equals("")){
                    // Insertar los datos de la ficha en la base de datos
                    mDb.insert(DBStructure.TABLE_FICHAS, null, values);
                } else {
                    String where = DBStructure.FICHAS_NFICHA + "=? and " + DBStructure.FICHAS_IDSALON + "=?";
                    mDb.update(DBStructure.TABLE_FICHAS, values, where, new String[]{ficha.nFichaAnterior, String.valueOf(ficha.idSalon)});
                }

                this.insertFichasLineas(ficha.lineas, ficha.nFichaAnterior);
            }

        } catch (Exception ex) {
            throw ex;
        }
    }

    private void insertFichasLineas(List<DatosFichaLinea> data, String nFichaAnterior) {
        try {
            for (DatosFichaLinea linea : data) {

                ContentValues values = new ContentValues();
                values.put(DBStructure.FICHASLINEAS_NFICHA, linea.nFicha);
                values.put(DBStructure.FICHASLINEAS_IDSALON, linea.idSalon);
                values.put(DBStructure.FICHASLINEAS_LINEA, linea.linea);
                values.put(DBStructure.FICHASLINEAS_CODIGO, linea.codigo);
                values.put(DBStructure.FICHASLINEAS_IDSERCIO, linea.idServicio);
                values.put(DBStructure.FICHASLINEAS_DESCRIPCION, linea.descripcion);
                values.put(DBStructure.FICHASLINEAS_BASE, linea.base);
                values.put(DBStructure.FICHASLINEAS_DESCUENTOPORC, linea.descuentoPorc);
                values.put(DBStructure.FICHASLINEAS_DESCUENTOCANT, linea.descuentoCant);
                values.put(DBStructure.FICHASLINEAS_IVAPORC, linea.ivaPorc);
                values.put(DBStructure.FICHASLINEAS_IVACANT, linea.ivaCant);
                values.put(DBStructure.FICHASLINEAS_TOTAL, linea.total);
                values.put(DBStructure.FICHASLINEAS_COMISIONP1, linea.comisionP1);
                values.put(DBStructure.FICHASLINEAS_COMISIONP2, linea.comisionP2);
                values.put(DBStructure.FICHASLINEAS_COMISIONP3, linea.comisionP3);
                values.put(DBStructure.FICHASLINEAS_COMISIONP4, linea.comisionP4);

                // Insertar los datos de la línea de ficha en la base de datos
                long result = mDb.insert(DBStructure.TABLE_FICHASLINEAS, null, values);
                if (result == -1){
                    String where = DBStructure.FICHAS_NFICHA + "=? and " + DBStructure.FICHAS_IDSALON + "=? and " + DBStructure.FICHASLINEAS_LINEA + "=?";
                    mDb.update(DBStructure.TABLE_FICHASLINEAS, values, where, new String[]{nFichaAnterior, String.valueOf(linea.idSalon), String.valueOf(linea.linea)});
                }
            }

        } catch (Exception ex) {
            throw ex;
        }
    }

    public void eliminarLínea(DatosFichaLinea linea){
        try {
            String where = DBStructure.FICHAS_NFICHA + "=? and " + DBStructure.FICHAS_IDSALON + "=? and " + DBStructure.FICHASLINEAS_LINEA + "=?";
            mDb.delete(DBStructure.TABLE_FICHASLINEAS, where, new String[]{linea.nFicha, String.valueOf(linea.idSalon), String.valueOf(linea.linea)});
        } catch (Exception ex){
            throw ex;
        }
    }
}
