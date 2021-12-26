currentMode = getCookie("dm") == "true";
DarkMode(currentMode);

function ToggleDM(){
	DarkMode(!currentMode);
}

function DarkMode(toggle)
{
	currentMode = toggle;
	if(currentMode == true){
  	setCookie("dm", "true", 365);
  	document.body.style.backgroundColor = "black";
    document.getElementById("container").style.filter = "invert(1)";
    document.getElementById("dmt").style.filter = "invert(1)";
  }
  else{
  	setCookie("dm", "false", 0);
  	document.body.style.backgroundColor = "white";
    document.getElementById("container").style.filter = "invert(0)";
    document.getElementById("dmt").style.filter = "invert(0)";
  }
}


function setCookie(cname, cvalue, exdays) {
  const d = new Date();
  d.setTime(d.getTime() + (exdays * 24 * 60 * 60 * 1000));
  let expires = "expires="+d.toUTCString();
  document.cookie = cname + "=" + cvalue + ";" + expires + ";path=/";
}

function getCookie(cname) {
  let name = cname + "=";
  let ca = document.cookie.split(';');
  for(let i = 0; i < ca.length; i++) {
    let c = ca[i];
    while (c.charAt(0) == ' ') {
      c = c.substring(1);
    }
    if (c.indexOf(name) == 0) {
      return c.substring(name.length, c.length);
    }
  }
  return "";
}