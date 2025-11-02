plugins {
    id("com.android.application")
}

android {
    namespace = "com.example.mikaapp"
    compileSdk = 34

    defaultConfig {
        applicationId = "com.example.mikaapp"
        minSdk = 33
        targetSdk = 33
        versionCode = 1
        versionName = "1.0"

        testInstrumentationRunner = "androidx.test.runner.AndroidJUnitRunner"
    }

    buildTypes {
        debug {
            resValue("string", "api_url", "https://test.ludiverso.com/api/Resources/")
        }
        release {
            isMinifyEnabled = false
            proguardFiles(
                getDefaultProguardFile("proguard-android-optimize.txt"),
                "proguard-rules.pro"
            )
            resValue("string", "api_url", "https://www.micapeluqueros.es/api/Resources/")
        }
    }
    compileOptions {
        sourceCompatibility = JavaVersion.VERSION_1_8
        targetCompatibility = JavaVersion.VERSION_1_8
    }

    applicationVariants.all {
        this.outputs
                .map { it as com.android.build.gradle.internal.api.ApkVariantOutputImpl }
                .forEach { output ->
                    val variant = this.buildType.name
                    var apkName = "MikaApp_" + this.versionName
                    if (variant.isNotEmpty()) apkName += "_$variant"
                    apkName += ".apk"
                    println("ApkName=$apkName ${this.buildType.name}")
                    output.outputFileName = apkName
                }
    }
}

dependencies {

    implementation("androidx.appcompat:appcompat:1.6.1")
    implementation("com.google.android.material:material:1.11.0")
    implementation("androidx.constraintlayout:constraintlayout:2.1.4")
    implementation("com.android.volley:volley:1.2.0")
    implementation("com.google.code.gson:gson:2.10.1")
    testImplementation("junit:junit:4.13.2")
    androidTestImplementation("androidx.test.ext:junit:1.1.5")
    androidTestImplementation("androidx.test.espresso:espresso-core:3.5.1")
}