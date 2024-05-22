package com.example.mikaapp;

import java.util.Date;

public class AuthenticateResponse {
    public boolean succeeded;
    public String token;
    public Date tokenExpires;
    public MikaWebUser userData;


}
