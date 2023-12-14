# 💾 Realtime Market Data Mock Server 💾
- 클라이언트에게 **대용량 시세, 뉴스 정보**를 **실시간**으로 전달하는 프로그램입니다. <br>
- UDP 프로토콜을 사용하며 클라이언트에게 정보 수신 요청을 받습니다.
- 정보 수신 요청한 클라이언트를 Observer 로 등록합니다.
- Observer 로 등록된 클라이언트에 한해서 데이터를 송신합니다.

---

**클래스 설명**
- **`Initialize()`**: 클라이언트 초기화 메서드로 서버 IP 및 포트로 연결할 소켓을 생성합니다.
- **`Subscribe()`**: 특정 서버에 연결하고 실시간 시세 정보를 요청합니다.
- **`StartMessageReceiveLoop()`**: 서버로부터 메시지를 수신하고 데이터를 파싱하여 주가 정보를 추출합니다. 주가 정보를 실시간으로 가져오는 역할을 수행합니다.
- **`UnSubscribe()`**: 실시간 시세 조회 요청을 해제하는 기능이 필요한데, 아직 미구현된 상태입니다.
- **`Close()`**: 클라이언트 소켓을 닫고 서버와의 연결을 종료합니다.

---

**UDP Payload 구조 설명**
- **Input:** 클라이언트에게서 받는 메시지 구조입니다.
<br>
<code>[-------------------전체 (4096byte)--------------------] </code><br>
<code>[ [요청 헤더(1byte)] [상세 내용(1024byte)] [기타(3071byte)] ]</code> <br><br>

- **`요청 헤더`**: 특정 값에 따라 서버와의 작업을 식별하며, 구분값은 아래 표 내용과 같습니다<br>
- **`상세 내용`**: 요청 헤더별 payload 가 입력되는 공간이며, 아직 미구현된 상태입니다.<br>
- **`기타`**: 인증, 체크섬, 기타 등의 정보가 입력되는 공간이며, 아직 미구현된 상태입니다.<br>

|구분|설명|
|------|---|
|0x01|서버 접속 / 서버 등록|
|0x10|실시간 시세 조회 요청 등록|
|0x20|실시간 시세 조회 요청 해제|
|0xFF|접속 종료 / 서버 등록 해제 요청|

- **Output Payload:** 등록된 Observer 에게 보내는 메시지 구조입니다.
<br>
<code>[-------------------전체 (4096byte)--------------------] </code><br>
<code>[ [요청 헤더(1byte)] [상세 내용(1024byte)] [기타(3071byte)] ]</code> <br><br>

- **`요청 헤더`**: 특정 값에 따라 서버와의 작업을 식별하며, 구분값은 아래 표 내용과 같습니다<br>
---

**실행방법**
1. 데이터 송신 서버가 기 동작 상태여야 합니다. 서버 프로그램은 [여기](https://github.com/wkjung0624/realtime-market-data-mock-server)를 클릭하시면 확인하실 수 있습니다.
2. Terminal 에서 <code>dotnet run</code> 을 입력합니다.
3. 동작 여부를 확인합니다.

<img width="890" alt="스크린샷 2023-12-14 오전 11 03 45" src="https://github.com/wkjung0624/realtime-market-data-receiver/assets/35141349/1f15e3ec-87fa-457a-b94b-c5af88988455">

