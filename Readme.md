## ToreAurstadIT.Logger - RollingXmlWriterTraceListener

Denne l�sningen inneholder en tilpasset logger for SVCLOG filer (.svclog) som viser svclogs.

Denne rullerer slik at filst�rrelsen p� hver aktive .svclog fil holdes nede. Eksempel p� config setting er denne brukt i SomeAcme: 

```	xml

<system.diagnostics>
    <sources>
      <source name="System.IdentityModel" switchValue="Warning" logKnownPii="false">
        <listeners>
          <add name="xml" />
        </listeners>
      </source>
      <source name="System.ServiceModel.MessageLogging" logKnownPii="false" switchValue="Warning">
        <listeners>
          <add name="xml" />
        </listeners>
      </source>
      <source name="System.ServiceModel" switchValue="Warning" propagateActivity="true">
        <listeners>
          <add name="xml" />
        </listeners>
      </source>
      <source name="Microsoft.IdentityModel" switchValue="Warning">
        <listeners>
          <add name="xml" />
        </listeners>
      </source>
    </sources>
    <sharedListeners>
       <!-- Using RollingXmlWriterTraceListener - Setting a max size of 100 MB -->
      <!-- <add type="ToreAurstadIt.Logger.RollingDateXmlWriterTraceListener, ToreAurstadIT.RollingSvcLogger" name="xml" initializeData="C:\svclogs\SomeAcmeTraceLog.svclog" MaxTraceFileSize="104857600" /> -->
      <add name="xml" type="System.Diagnostics.XmlWriterTraceListener" initializeData="C:\temp\SomeAcmeWAS.svclog" />
    </sharedListeners>
    <trace autoflush="true" />
  </system.diagnostics>


```

Bemerk switchValue lik "Warning" her. Vi �nsker ikke "Information", siden det vil sende alt for mye informasjon til svclog. Kun "Warning" skal benyttes for switchValue . 

Her RollingXmlWriterTracerListener kommentert ut og man bruker standard System.Diagnostics.XmlWriterTraceListener. Her s� er en oppdatert config for SomeAcme som tar i bruk RollingXmlWriterTraceListener:


```	xml

<system.diagnostics>
    <sources>
      <source name="System.IdentityModel" switchValue="Warning" logKnownPii="false">
        <listeners>
          <add name="xml" />
        </listeners>
      </source>
      <source name="System.ServiceModel.MessageLogging" logKnownPii="false" switchValue="Warning">
        <listeners>
          <add name="xml" />
        </listeners>
      </source>
      <source name="System.ServiceModel" switchValue="Warning" propagateActivity="true">
        <listeners>
          <add name="xml" />
        </listeners>
      </source>
      <source name="Microsoft.IdentityModel" switchValue="Warning">
        <listeners>
          <add name="xml" />
        </listeners>
      </source>
    </sources>
    <sharedListeners>
      <!--<add name="xml" type="System.Diagnostics.XmlWriterTraceListener" initializeData="C:\svclogs\SomeAcme.svclog" />-->
      <!-- Using RollingXmlWriterTraceListener - Setting a max size of 1000 MB, see calculator here: https://www.gbmb.org/mb-to-bytes -->
      <add type="ToreAurstadIt.Logger.RollingDateXmlWriterTraceListener, ToreAurstadIT.RollingSvcLogger" name="xml" initializeData="C:\svclogs\SomeAcmeTraceLog.svclog" MaxTraceFileSize="104857600" /> 
    </sharedListeners>
    <trace autoflush="true" />
  </system.diagnostics>


```

I configen ovenfor s� setter vi 1000 MB i st�rrelse via MaxTraceFileSize argumentet.

For � teste ut loggeren kan vi sette MaxTraceFileSize til en lav verdi, 300000 , 300 KB: 

```xml

    <sharedListeners>
        <!-- Using RollingXmlWriterTraceListener - Setting a max size of 100 MB -->
      <!--<add type="ToreAurstadIT.Logger.RollingXmlWriterTraceListener, ToreAurstadIT.Logger" name="xml" initializeData="C:\svclogs\SomeAcmeTraceLog.svclog" MaxTraceFileSize="300000" /> -->
      <add name="xml" type="System.Diagnostics.XmlWriterTraceListener" initializeData="C:\temp\SomeAcmeWAS_RollingLog.svclog" MaxTraceFileSize="300000" />  
    </sharedListeners>
```

Dette vil gi oss en _rullerende logg med maks 300 KB i hver fil_ , hvor hver enkelt .svclog fil holdes nede i st�rrelse. Det er kun den .svclog fila som er i bruk som er l�st av prosessen som bruker
loggen. Dette gj�r det lettere for ToreAurstadIT-Drift � holde logg st�rrelsen nede i st�rrelse. 

Den rullerende loggen setter opp flere filer med konfigurert maks st�rrelse. Dette gj�r det lettere � holde kontroll p� svclogs filene.

![Svclog files in dir](images/svclog_files_dir.png)



For ToreAurstadIT Drift b�r feks 1 GB v�re en grei innstilling. Samtidig er jo kanskje 100 MB ogs� en grei innstilling hvis man vil ha 
raskere svclog filer � lese ut.


100 MB :

```xml

    <sharedListeners>
      <!-- Using RollingXmlWriterTraceListener - Setting a max size of 100 MB -->
      <add type="ToreAurstadIT.Logger.RollingXmlWriterTraceListener, ToreAurstadIT.Logger" name="xml" initializeData="C:\svclogs\SomeAcmeTraceLog.svclog" MaxTraceFileSize="104857600" /> 
      <!-- <add name="xml" type="System.Diagnostics.XmlWriterTraceListener" initializeData="C:\temp\SomeAcmeWAS_RollingLog.svclog" MaxTraceFileSize="104857600" /> -->
    </sharedListeners>
```

1 GB :

```xml

    <sharedListeners>
      <!--<add name="xml" type="System.Diagnostics.XmlWriterTraceListener" initializeData="C:\svclogs\SomeAcmeWAS.svclog" />-->
      <!-- Using RollingXmlWriterTraceListener - Setting a max size of 1 GB-->
      <add type="ToreAurstadIT.Logger.RollingXmlWriterTraceListener, ToreAurstadIT.Logger" name="xml" initializeData="C:\svclogs\SomeAcmeTraceLog.svclog" MaxTraceFileSize="1048576000 " /> 
      <!-- <add name="xml" type="System.Diagnostics.XmlWriterTraceListener" initializeData="C:\temp\SomeAcmeWAS_RollingLog.svclog" MaxTraceFileSize="1048576000" /> -->
    </sharedListeners>
```


Bemerk - ved sletting av enkeltvise .svclog filer s� m� applikasjonsloggen recycles rett etterp� for � unng� feil i loggingen, hvis man sletter den .svclog fila med h�yeste
nummer. I tillegg b�r man unng� slette den tomme .svclog filen som dukker opp med konfigurert filnavn i _initializeData_ og den som er nummerert 0000 . 

Det anbefales � sette opp 500 MB default i maks fil st�rrelse f�r man rullerer over p� neste fil, dette for � s�rge for at .svclog filene holdes nede i st�rrelse og er noenlunde h�ndterlige � �pne i _SvcTraceViewer_ i utviklingsmilj�.

Denne .svclog filen feks viser en feil (Error) som har skjedd i en .svclog ved at man �pner loggen i Svc Trace Viewer. Dette verkt�yet f�r man ved � laste ned Windows SDK. Kryss for � f� med verkt�yet n�r du installerer Windows SDK.

![Svclog files in dir](images/svclog_file_error.png)


Sist oppdatert : 

06.10.2023

Tore Aurstad
tore.aurstad@ToreAurstadIT.no 
Senior Systemutvikler ToreAurstadIT SU