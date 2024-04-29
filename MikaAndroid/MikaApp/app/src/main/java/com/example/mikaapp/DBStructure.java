package com.example.mikaapp;

public class DBStructure {

    private DBStructure(){}

    public static final String TABLE_USUARIOS = "AspNetUsers";
    public static final String TABLE_CLIENTES = "Clientes";
    public static final String TABLE_EMPLEADOSCOMISIONES = "EmpleadosComisiones";
    public static final String TABLE_EMPLEADOSSERVICIOS = "EmpleadosServicios";
    public static final String TABLE_EMPRESAS = "Empresas";
    public static final String TABLE_SALONES = "Salones";
    public static final String TABLE_SERVICIOS = "Servicios";

    public static final String USUARIOS_ID = "Id";
    public static final String USUARIOS_USERNAME = "UserName";
    public static final String USUARIOS_EMAIL = "Email";
    public static final String USUARIOS_SECURITYSTAMP = "SecurityStamp";
    public static final String USUARIOS_PHONENUMBER = "PhoneNumber";
    public static final String USUARIOS_ACTIVO = "Activo";
    public static final String USUARIOS_APELLIDOS = "Apellidos";
    public static final String USUARIOS_NOMBRE = "Nombre";
    public static final String USUARIOS_ISADMIN = "IsAdmin";
    public static final String USUARIOS_CODIGO = "Codigo";
    public static final String USUARIOS_SALON = "Salon";


    public static final String EMPRESAS_IDEMPRESA = "IdEmpresa";
    public static final String EMPRESAS_NOMBRE = "Nombre";
    public static final String EMPRESAS_CIF = "CIF";
    public static final String EMPRESAS_DIRECCION = "Direccion";
    public static final String EMPRESAS_CP = "CP";
    public static final String EMPRESAS_CIUDAD = "Ciudad";
    public static final String EMPRESAS_TELEFONO = "Telefono";
    public static final String EMPRESAS_EMAIL = "Email";

}
