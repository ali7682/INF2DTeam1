import asyncio
import aiohttp
import time
from statistics import mean

# === CONFIGUREER DIT ===
API_URL = "http://localhost:8000/login"  # pas aan naar jouw endpoint

# Inloggegevens (username;password)
raw_users = """
cindyleenders42;6b37d1ec969838d29cb611deaff50a6b
gijsdegraaf;1b1f4e666f54b55ccd2c701ec3435dba
iris.dekker70;bf7ea48e511957eccb06a832ba6ae6c9
jan.schouten;bdafaec87e1ae7859dce625dfedaffb7
willem.dijkstra;77331e3916dedb5048c0c10ad7aa3d52
lara.veen;875e45bb6724ce82ffa42d9b7d17a049
caroline.visser;9e39c78e97829a846be4d67d8b98d920
jeroen.vos69;9e39c78e97829a846be4d67d8b98d920
lara.postma;be961c906e3b375dced446d4cf0b6856
iris.vanwijk;cddbd53f6928a2bc90eda6811da16b6c
lisa.vanleeuwen59;d5ecf117277a8a3a392b78a86f57dc70
thijs.willems80;3a3abc0c86dd488f9a05d3c6d55803ef
pieterbakker;698f7d405956b0c09b3ef547996d33b7
vera.vanderlaan;c46e14d2acaa744facf4a4e4699cc185
kai.willems;b9fa8a2582c10377d75b449b3c3bbd1a
fleur.dehaan;6390462745a49ec64b4613388639011b
anna.blom;4299b5e55e0a6f323ec000a274347dcd
iris.gelderen;5a21a176b55a49d9f87ea879fd7683e8
milanvanderlaan;7cbe63ea35a1b8c354a7f26d0fcd2dd3
tess.adriaanse86;33be7a3ebaaa13f3b4dd82b776acb24f
arjanvanderheijden;1df83c4ea948547e7aa9cfd6f4b48a07
arjan.berg;9299b8fb35782a21fe6ef913893a554b
rob.vos;3e633fa3a830ae6531d3be990a3ffeb1
isa.smeets;834696d930bb07c1dd0be8e073e0bc2f
petrapeters16;0f40bcef0a703cb88a2fa815db1e30a5
jeroen.jacobs;fb2658784c7b92a1bf28ffda23a1cd4f
luuk.verhoeven;6dae46591192a45eedecbb203b612700
evi196850;495fd8c0057a0966ca1f54b6a343d19c
teun1991;c95c607d0a71f0618e157285fca406a2
fenna.bos8;ec5681f99271a73375e2f452477ea574
svendevries;7c96e89df1bb47fb376645fa60561c5b
tim.scholten9;6925483f9ecedaf8c7560ba15dfa041b
milawillems;6716686259c296138760bc8ab7913944
mirandaleenders;e2be63b5c203447f9cfeaa0c66926ee4
saar.koning;926742e502de7d22686bb1d4a07fe635
danielle.boer;055002ea7ebea8e4bfca172606232407
hans.meer;3e318de8555bc44b635d0ace76e01420
evakoster;7349a0a5260982c12ac892d90c705c7d
tess.devos91;6249dc5c5e6c7091e352ead98c9b128b
ryan1949;b2e330cd06fbdc8dbe01037f6fb83135
boaz.bakker95;edec827bb637830b9e18f0eb9c325ece
kees.deridder;3aa4d37f98c2f70eda516072a77bb1bb
pip.smit;464739ab58603d8e183b9d0e2bc7f657
chantal.dehaan;3e6f4c45b85be86d1334ed3b395a478a
emma.postma;054640aad9d864822dd6fed2282b6755
kevinvanderlaan;77331e3916dedb5048c0c10ad7aa3d52
linda.devries;f0864faf9f25fad69762d7760d7a9ba2
ronald.pieters;3252ac7e65282d373e3aba432ee53870
fleurjansen;2fff5ef2a1c6fde9f1a53c22bcdeae64
olivier.kuipers75;cfb20976542b1fc5b82e229974b48934
""".strip().splitlines()

# Parse
users = [line.split(";") for line in raw_users]

async def test_login(session, username, password):
    start = time.perf_counter()
    try:
        async with session.post(API_URL, json={"username": username, "password": password}) as resp:
            duration = (time.perf_counter() - start) * 1000
            status = resp.status
            text = await resp.text()
            print(f"{username:25s} | {status} | {duration:7.2f} ms")
            return duration
    except Exception as e:
        print(f"{username:25s} | ERROR | {e}")
        return None

async def main():
    async with aiohttp.ClientSession() as session:
        tasks = [
            test_login(session, u, p)
            for u, p in users[:5]  # 50 tegelijk
        ]
        durations = await asyncio.gather(*tasks)
        durations = [d for d in durations if d is not None]

        if durations:
            print("\n=== Samenvatting ===")
            print(f"Aantal geslaagde requests: {len(durations)}")
            print(f"Gemiddelde duur: {mean(durations):.2f} ms")
            print(f"Snelste: {min(durations):.2f} ms")
            print(f"Traagste: {max(durations):.2f} ms")
        else:
            print("Geen geldige resultaten ontvangen.")

if __name__ == "__main__":
    asyncio.run(main())
