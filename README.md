"# CubLearn" 

##### …or create a new repository on the command line
```Shell {.line-numbers}
echo "# CubLearn" >> README.md
git init
git add README.md
git commit -m "first commit"
git remote add origin https://github.com/carpmhw/CubLearn.git
git push -u origin master
```

```Shell {.line-numbers}
sh /jboss/jboss-eap-7.2/bin/jboss-cli.sh --connect --controller=127.0.0.1:9990

/server-group=batch-server-group/deployment=dph-batch:read-resource

確認service狀態
ls /host=master/server-config=server-batch-one

確認jboss status
/usr/bin/systemctl status jboss.service

確認9990的連線
netstat -anp | grep :9990 | grep ESTABLISHED | wc -l
```

> netsh interface ipv4 show excludedportrange protocol=tcp

> https://my-json-server.typicode.com/<your-username\>/<your-repo\>