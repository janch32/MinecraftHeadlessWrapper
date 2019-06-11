var ws;
var managerList = document.getElementById("managerList");
var managerSelect = document.getElementById("managerSelect");
managerSelect.addEventListener("click", createServer);
managerList.addEventListener("keyup", createServer);
function createServer(e){
	if(e && e.keyCode && e.keyCode != 13) return;
	document.body.classList.remove("requireManager");
	//ws = new WebSocket("ws://" + managerList.value + "/remote-manager");
	ws = new WebSocket("ws://" + location.host + ":25500/remote-manager");
	//var ws = new WebSocket("wss://localhost:25500/remote-manager");
	ws.onopen = function(){
		console.log("WS opened!");
	}

	ws.onclose = function(){
		console.log("WS closed!");
	}

	ws.onmessage = function(e){
		//console.log("WS data receive!");
		var msg = JSON.parse(e.data);
		//console.log(msg);

		switch (msg.Type) {
			case "AuthRequired":
				document.body.classList.add("requireLogin");
				break;
			case "AuthIncorrect":
				alert("Chybné heslo");
				break;
			case "AuthCorrect":
				document.body.classList.remove("requireLogin");
				break;
			case "ServerList":
				msg.ServersInfo.forEach(e => {
					updateServerInfo(e);
				});
				break;
			case "ServerInfo":
				updateServerInfo(msg.Info);
				break;
			case "ServerConsoleLines":
				consoleLog.innerText = msg.Lines.join("\n");
				consoleLog.scrollTop = consoleLog.scrollHeight - consoleLog.clientHeight;
				break;
			case "ServerNewLine":
				consoleAddNewLine(msg.NewLine);
				break;
			default:
				break;
		}
	}
}
createServer();

function SendMessage(msg){
	ws.send(JSON.stringify(msg));
}

var loginUser = document.getElementById("username");
var loginPass = document.getElementById("password");
var loginBtn = document.getElementById("login");
loginUser.addEventListener("keyup", tryLogin);
loginPass.addEventListener("keyup", tryLogin);
loginBtn.addEventListener("click", tryLogin);

function tryLogin(e){
	if(e.keyCode == null || e.keyCode == 13){
		SendMessage({Type: "AuthRequest", Username: loginUser.value, Password: loginPass.value});
	}
}

var uptime = document.getElementById("subscribedStatus");
function updateServerInfo(details){
	var el = getElementByServerName(details.Name);
	if(!el){
		addServerToServerList(details);
		return;
	}

	var isSub = el.classList.contains("subscribed");
	el.className = "serverListItem " + (isSub?"subscribed ":"") + details.Status.toLowerCase();
	if(isSub){
		changeStatusButtons();
		consoleCommand.disabled = true;
		if(details.Status == "Starting" || details.Status == "Running"){
			consoleCommand.disabled = false;
			uptime.innerText = "Server běží " + prettyPrintDate(Date.parse(details.StartTime));
		}else if(details.Status == "Stopping")
			uptime.innerText = "Server se vypíná...";
		else if(details.Status == "Inactive")
			uptime.innerText = "Server je vypnutý";
		else if(details.Status == "InactiveFail")
			uptime.innerText = "Server je vypnutů (skončil s chybou)";
		else if(details.Status == "PensingRestart")
			uptime.innerText = "Server se restartuje...";
	} 

	var memUsage = el.getElementsByClassName("memoryUsage");
	if(memUsage.length > 0){
		memUsage[0].innerText = details.MemoryUsage + "M";
		if(details.MaxMemoryUsage > 0)
			memUsage[0].innerText += " / " + details.MaxMemoryUsage + "M";
	}

	var memBar = el.getElementsByClassName("memoryBar");
	if(memBar.length > 0){
		memBar[0].max = details.MaxMemoryUsage;
		memBar[0].value = details.MemoryUsage;
	}
}

function getElementByServerName(servername){
	var serverList = document.getElementById("serverList");
	if(!serverList) return false;
	
	var items = serverList.getElementsByClassName("serverListItem");
	for (var el of items) {
		if(el.getAttribute("servername") == servername){
			return el;
		}
	}

	return false;
}

function addServerToServerList(details){
	var startBtn = document.createElement("div");
	startBtn.className = "start";
	startBtn.title = "Spustit";
	startBtn.addEventListener("click", changeServerStatus);
	
	var restartBtn = document.createElement("div");
	restartBtn.className = "restart";
	restartBtn.title = "Restartovat";
	restartBtn.addEventListener("click", changeServerStatus);
	
	var stopBtn = document.createElement("div");
	stopBtn.className = "stop";
	stopBtn.title = "Zastavit";
	stopBtn.addEventListener("click", changeServerStatus);
	
	var actionDiv = document.createElement("div");
	actionDiv.className = "action";
	actionDiv.appendChild(restartBtn);
	actionDiv.appendChild(startBtn);
	actionDiv.appendChild(stopBtn);
	
	var statusDiv = document.createElement("div");
	statusDiv.className = "status";

	var nameDiv = document.createElement("div");
	nameDiv.className = "name";
	nameDiv.innerText = details.Name;

	var memUsageDiv = document.createElement("span");
	memUsageDiv.className = "memoryUsage";
	memUsageDiv.innerText = details.MemoryUsage + "M";
	if(details.MaxMemoryUsage > 0)
		memUsageDiv.innerText += " / " + details.MaxMemoryUsage + "M";

	var memBar = document.createElement("progress");
	memBar.className = "memoryBar";
	memBar.value = details.MemoryUsage;
	memBar.max = details.MaxMemoryUsage;

	var serverListItem = document.createElement("div");
	serverListItem.className = "serverListItem " + details.Status.toLowerCase();
	serverListItem.setAttribute("servername", details.Name);
	serverListItem.addEventListener("click", subscribeServer);
	serverListItem.appendChild(nameDiv);
	serverListItem.appendChild(statusDiv);
	serverListItem.appendChild(actionDiv);
	serverListItem.appendChild(memUsageDiv);
	serverListItem.appendChild(memBar);

	document.getElementById("serverList").appendChild(serverListItem);
}

var subscribedServerName = document.getElementById("subscribedName");
function subscribeServer(){
	var subOld = document.getElementsByClassName("subscribed");
	for (e of subOld) {
		e.classList.remove("subscribed");
	}
	
	subscribedServerName.innerText = this.getAttribute("servername");
	this.classList.add("subscribed");
	document.body.className = "";
	changeStatusButtons();
	SendMessage({Type: "SubscribeServer", Name: this.getAttribute("servername")});
	SendMessage({Type: "RequestServerList"});
	consoleCommand.value = "";
	consoleCommand.focus();
}

var consoleLog = document.getElementById("consoleLog");
function consoleAddNewLine(line){
	var isScrolled = consoleLog.scrollTop == consoleLog.scrollHeight - consoleLog.clientHeight;
	consoleLog.append("\n" + (line != null ? line : ""));
	if(isScrolled)
		consoleLog.scrollTop = consoleLog.scrollHeight - consoleLog.clientHeight;
}

var consoleCommand = document.getElementById("consoleCommand");
consoleCommand.addEventListener("keyup", sendCommand);
function sendCommand(e){
	if(e.keyCode == 13){
		SendMessage({Type: "ServerCommand", Command: this.value});
		this.value = "";
	}
}

var subscribedRestart = document.getElementById("subscribedRestart")
var subscribedStop = document.getElementById("subscribedStop")
var subscribedStart = document.getElementById("subscribedStart")
subscribedRestart.addEventListener("click", changeServerStatus);
subscribedStop.addEventListener("click", changeServerStatus);
subscribedStart.addEventListener("click", changeServerStatus);
function changeServerStatus(e){
	e.stopPropagation();
	if(!confirm("Opravdu chcete provést tuto akci?")) return;
	
	var name = this.parentElement.parentElement.getAttribute("servername");
	if(!name){
		var sub = document.getElementsByClassName("subscribed");
		if(sub.length <= 0) return;
		name = sub[0].getAttribute("servername");
	}

	if(this.classList.contains("start"))
		SendMessage({Type: "StartServer", Name: name});
	else if(this.classList.contains("stop"))
		SendMessage({Type: "StopServer", Name: name});
	else
		SendMessage({Type: "RestartServer", Name: name});
}

setInterval(function(){
	if(!document.body.classList.contains("requireLogin"))
		SendMessage({Type: "RequestServerList"});
}, 20000);

function changeStatusButtons(){
	var subel = document.getElementsByClassName("subscribed");
	
	subscribedStart.style.display = "none";
	subscribedRestart.style.display = "none";
	subscribedStop.style.display = "none";
	if(subel.length > 0){
		if(subel[0].classList.contains("starting") || subel[0].classList.contains("running")){
			subscribedStop.style.display = "block";
			subscribedRestart.style.display = "block";
		}else if(subel[0].classList.contains("inactive") || subel[0].classList.contains("inactivefail")){
			subscribedStart.style.display = "block";
		}
	}
}

function prettyPrintDate(date){
	var diff = Math.floor((Date.now() - date) / 60000);
	
	var minute = diff % 60;
	diff = Math.floor(diff / 60);
	var hour = diff % 60;
	var day = Math.floor(diff / 60);

	var str = [];
	/*if(year == 1) str.push("rok");
	else if(year < 5 && year > 1) str.push(year + " roky");
	else if(year > 0) str.push(year + " let");

	if(month == 1) str.push("měsíc");
	else if(month < 5 && month > 1) str.push(month + " měsíce");
	else if(month > 0) str.push(month + " měsíců");
	*/
	if(day == 1) str.push("den");
	else if(day < 5 && day > 1) str.push(day + " dny");
	else if(day > 0) str.push(day + " dnů");

	if(hour == 1) str.push("hodinu");
	else if(hour < 5 && hour > 1) str.push(hour + " hodiny");
	else if(hour > 0) str.push(hour + " hodin");

	if(minute == 1) str.push("minutu");
	else if(minute < 5 && minute > 1) str.push(minute + " minuty");
	else if (minute > 0) str.push(minute + " minut");
	
	if (str.length == 0) str.push("méně než minutu");

	var last = str.pop();

	var final = str.join(", ");
	if(final) final += " a ";
	return final + last;
}