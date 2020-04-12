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

/server-group=alipay-server-group:stop-servers

/server-group=alipay-server-group:start-servers

/server-group=batch-server-group/deployment=dph-batch:read-resource

確認service狀態
ls /host=master/server-config=server-batch-one

確認jboss status
/usr/bin/systemctl status jboss.service

確認9990的連線
netstat -anp | grep :9990 | grep ESTABLISHED | wc -l

linux
此檔案可用來設定 DNS 用戶端要求名稱解析時，所定義的各項內容
cat /etc/resolv.conf

```

> netsh interface ipv4 show excludedportrange protocol=tcp

> https://my-json-server.typicode.com/<your-username\>/<your-repo\>

``` Shell {.line-numbers}

列出傳輸時間超過 30 秒的檔案
cat access_log | awk '($NF > 30){print $7}' | sort -n | uniq -c | sort -nr | head -20

列出最最耗時的頁面(超過60秒的)
cat access_log | awk '($NF > 60 && $7~/\.php/){print $7}' | sort -n | uniq -c | sort -nr | head -100

檢視日誌中訪問次數最多的前10個IP
cat access_log |cut -d ' ' -f 1 | sort |uniq -c | sort -nr | awk '{print $0 }' | head -n 10 | less

檢視日誌中出現100次以上的IP
cat access_log |cut -d ' ' -f 1 | sort |uniq -c | awk '{if ($1 > 100) print $0}'｜sort -nr | less
```