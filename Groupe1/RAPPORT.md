### **📄 Rapport de Projet**

📅 **Date** : 08/02/2025
👥 **Équipe** : *(Abderrachid BELLAALI, Badr BADRANE, Cedric BRASSARD, Haseebullah BABURI, Nassim El Grari (Na551m89))*

---

## **1️⃣ Introduction**

Dans ce projet, nous avons développé un **chargeur de shellcode en C#**, permettant l’exécution d’un payload en mémoire en utilisant différentes méthodes, notamment `VirtualAlloc()`, `VirtualProtect()` et `NtCreateThreadEx()`.

Nous avons suivi plusieurs étapes pour générer, injecter et executer un shellcode de manière furtive, tout en évitant les détections des antivirus et des solutions EDR.

---

## **2️⃣ Objectifs**

-  Générer un shellcode avec msfvenom et tester différents formats et encodages pour échapper aux signatures AV.
-  Développer un loader en C# pour exécuter le shellcode en mémoire sans toucher au disque.
-  Explorer plusieurs techniques d’injection (VirtualAllocEx(), NtCreateThreadEx(), QueueUserAPC(), syscalls).
-  Optimiser la furtivité en chiffrant le shellcode, obfusquant les API sensibles et patchant la télémétrie Windows (ETW, AMSI).
-  Tester la détection AV/EDR via VirusTotal et des solutions comme Defender, CrowdStrike, SentinelOne.
-  Documenter les méthodes utilisées, comparer leur efficacité et proposer des améliorations pour une meilleure furtivité.


---

## **3️⃣ Étapes réalisées**

### **🛠 Génération du shellcode en C#**

- Utilisation de **Metasploit (**``**)** pour générer le payload :
  ```sh
  msfvenom -p windows/x64/messagebox TEXT="Test en mémoire" TITLE="Shellcode" -f csharp
  ```
- Analyse des formats disponibles (`--list formats`).
- Tests avec différents payloads (`calc.exe`, `NOP sled`, `MessageBox`).

---


#### **Méthodes utilisées**

### 1️⃣ Allocation et protection mémoire  
- `VirtualAlloc()` pour allouer une zone mémoire en **lecture/écriture (RW)**.  
- `VirtualProtect()` pour modifier la protection en **lecture/exécution (RX)** après injection, réduisant la détection.  

### 2️⃣ Injection et exécution furtive  
- `CreateThread()` utilisé dans la première version.  
- Remplacement par `NtCreateThreadEx()` pour **contourner les mécanismes de détection des EDR**.  

### 3️⃣ Obfuscation et évasion des défenses  
- **Chiffrement XOR du shellcode** pour éviter les signatures AV.  
- **Passage de RWX → RX** après écriture pour éviter les comportements suspects détectés par les EDR.  
- **Utilisation de syscalls directs** pour contourner le hooking des API critiques par les solutions de sécurité avancées.

---

### **🛠 Tests **

- Tests en **machine virtuelle** pour éviter les alertes AV.

---

#### **Liens avec le rapport "The Most Prevalent MITRE ATT&CK" **

| **Technique** | **Référence  MITRE** | **Liens avec les exercices** |
|--------------|---------------|--------------------------|
| **Injection de code dans un autre processus** | `T1055 – Process Injection` | Le 1er exercice injecte du shellcode dans un processus (`notepad.exe` ou `explorer.exe`). |
| **Exécution de code en mémoire** | `T1620 – Reflective Code Loading` | Le shellcode est exécuté **sans écrire de fichier sur le disque** (évasion des antivirus). |
| **Manipulation des permissions mémoire** | `T1106 – Memory Permissions Modification` | L’utilisation de `VirtualProtectEx()` et `NtAllocateVirtualMemory()` pour rendre le shellcode exécutable après l’injection. |
| **Evasion des défenses** | `T1202 – Indirect System Calls` | L’utilisation de `NtCreateThreadEx()` au lieu de `CreateRemoteThread()` contourne les détections heuristiques des EDR. |
| **Patching de télémétrie Windows** | `T1562.008 – Disable or Modify System Logging` | Le dernier exercice **patch** `EtwEventWrite()` pour empêcher l'EDR de capturer les logs d’exécution en mémoire. |
| **Chiffrement et obfuscation du shellcode** | `T1027 – Obfuscated Files or Information` | Le dernier exercice **chiffre le shellcode avec XOR** pour masquer sa signature et empêche sa détection par AV/EDR. |
| **Désactivation d'AMSI (Antimalware Scan Interface)** | `T1562.001 – Disable or Modify Tools` | En patchant `AMSI.dll`, l’exercice empêche Windows Defender de scanner les charges malveillantes. |
| **Détection d’un environnement sandbox/EDR** | `T1497 – Virtualization/Sandbox Evasion` | La vérification de la présence de **`vmtoolsd.exe`** pour éviter l'exécution dans une machine virtuelle/sandbox. |
| **Injection furtive via des appels directs** | `T1621 – Indirect Syscalls` | L’utilisation des **syscalls directs (`NtWriteVirtualMemory`, `NtAllocateVirtualMemory`)** pour éviter les hooks EDR. |

## **4️⃣ Réflexions individuelles**


### **👤 Membre 1 (Abderrachid)**

> *

### **👤 Membre 2 (Badr)**

> *

### **👤 Membre 3 (Cedric)**

> *

### **👤 Membre 4 (Haseebullah)**

> *


### **👤 Membre 5 (Nassim)**

> En tant que développeur de l'équipe, ce projet m'a permis de découvrir et d'explorer des concepts avancés de sécurité offensive et d'injection de shellcode en mémoire. Ne venant pas du monde de la sécurité offensive, j’ai dû apprendre et comprendre progressivement plusieurs notions essentielles.

> Ce projet a été une découverte enrichissante, où j’ai pu passer de l’injection naïve d’un shellcode à des techniques avancées d’évasion des EDR.
J’ai appris l’importance de comprendre la télémétrie Windows, les API critiques surveillées et les différentes méthodes de chiffrement et d’obfuscation.

---

## **5️⃣ Conclusion et Améliorations Futures**

## 📌 Points forts du projet

- **Expérience avec les API Windows**  
  - Utilisation avancée de `VirtualAlloc()`, `VirtualProtect()`, `NtCreateThreadEx()`, et des syscalls directs.  

- **Optimisation de l’exécution en mémoire**  
  - Techniques de furtivité pour éviter la détection par les AV et EDR.  
  - Chiffrement et obfuscation du shellcode pour contourner les signatures statiques.  

- **Tests sur plusieurs configurations**  
  - Évaluation avec et sans antivirus (Windows Defender, VirusTotal).  
  - Exécution sur différentes versions de Windows (Windows 10, Windows 11).  
  - Comparaison des détections avec et sans `NtCreateThreadEx()`.  

## 🚀 Améliorations possibles

- **Chiffrement AES du shellcode**  
  - Remplacer l’encodage XOR par **AES** pour une meilleure furtivité et éviter la détection par les AV basés sur les signatures.  

- **Automatisation avec PowerShell**  
  - Charger dynamiquement le shellcode via **PowerShell Reflective Injection**, permettant d'exécuter le payload sans fichier sur disque.  

- **Utilisation des appels système indirects (Indirect Syscalls)**  
  - Implémenter des **syscalls indirects** pour éviter le hooking des API Windows par les EDR.  

- **Amélioration de l’évasion des sandbox**  
  - Détection avancée des **VM et sandboxes** via des artefacts système (ex: `vmtoolsd.exe`, `SystemBiosVersion`).  

- **Exécution via Process Hollowing ou Thread Hijacking**  
  - Remplacer l’injection classique par **Process Hollowing** ou **Thread Hijacking** pour un meilleur contournement des EDR.  

---