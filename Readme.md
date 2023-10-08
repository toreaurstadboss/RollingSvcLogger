## ToreAurstadIT.Logger - RollingXmlWriterTraceListener

Denne løsningen inneholder en tilpasset logger for SVCLOG filer (.svclog) som viser svclogs.

Denne rullerer slik at filstørrelsen på hver aktive .svclog fil holdes nede. Eksempel på config setting er denne brukt i SomeAcme: 

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

Bemerk switchValue lik "Warning" her. Vi ønsker ikke "Information", siden det vil sende alt for mye informasjon til svclog. Kun "Warning" skal benyttes for switchValue . 

Her RollingXmlWriterTracerListener kommentert ut og man bruker standard System.Diagnostics.XmlWriterTraceListener. Her så er en oppdatert config for SomeAcme som tar i bruk RollingXmlWriterTraceListener:


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

I configen ovenfor så setter vi 1000 MB i størrelse via MaxTraceFileSize argumentet.

For å teste ut loggeren kan vi sette MaxTraceFileSize til en lav verdi, 300000 , 300 KB: 

```xml

    <sharedListeners>
        <!-- Using RollingXmlWriterTraceListener - Setting a max size of 100 MB -->
      <!--<add type="ToreAurstadIT.Logger.RollingXmlWriterTraceListener, ToreAurstadIT.Logger" name="xml" initializeData="C:\svclogs\SomeAcmeTraceLog.svclog" MaxTraceFileSize="300000" /> -->
      <add name="xml" type="System.Diagnostics.XmlWriterTraceListener" initializeData="C:\temp\SomeAcmeWAS_RollingLog.svclog" MaxTraceFileSize="300000" />  
    </sharedListeners>
```

Dette vil gi oss en _rullerende logg med maks 300 KB i hver fil_ , hvor hver enkelt .svclog fil holdes nede i størrelse. Det er kun den .svclog fila som er i bruk som er låst av prosessen som bruker
loggen. Dette gjør det lettere for ToreAurstadIT-Drift å holde logg størrelsen nede i størrelse. 

Den rullerende loggen setter opp flere filer med konfigurert maks størrelse. Dette gjør det lettere å holde kontroll på svclogs filene.

![Svclog files in dir](images/svclog_files_dir.png)



For ToreAurstadIT Drift bør feks 1 GB være en grei innstilling. Samtidig er jo kanskje 100 MB også en grei innstilling hvis man vil ha 
raskere svclog filer å lese ut.


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


Bemerk - ved sletting av enkeltvise .svclog filer så må applikasjonsloggen recycles rett etterpå for å unngå feil i loggingen, hvis man sletter den .svclog fila med høyeste
nummer. I tillegg bør man unngå slette den tomme .svclog filen som dukker opp med konfigurert filnavn i _initializeData_ og den som er nummerert 0000 . 

Det anbefales å sette opp 500 MB default i maks fil størrelse før man rullerer over på neste fil, dette for å sørge for at .svclog filene holdes nede i størrelse og er noenlunde håndterlige å åpne i _SvcTraceViewer_ i utviklingsmiljø.

Denne .svclog filen feks viser en feil (Error) som har skjedd i en .svclog ved at man åpner loggen i Svc Trace Viewer. Dette verktøyet får man ved å laste ned Windows SDK. Kryss for å få med verktøyet når du installerer Windows SDK.

![Svclog files in dir](images/svclog_file_error.png)


Sist oppdatert : 

06.10.2023

Tore Aurstad
tore.aurstad@ToreAurstadIT.no 
Senior Systemutvikler ToreAurstadIT SU