# MeshSparklyEffect

## 概要

![踊るパイモン](./Assets/MeshSparklyEffect/Documentation/Images/pymon_demo.gif)

指定したメッシュの頂点位置からテクスチャ色に合ったキラキラ感のあるパーティクルを出すUnity用のエフェクトです。

パーティクルには、十字に伸びるプロシージャルなものと、テクスチャの2種類を使用できます。

内部ではメッシュを指定した際に頂点情報をテクスチャに焼き込んでおり、焼き込んだテクスチャをEXRファイルとして保存することも可能です。

指定するメッシュの形式はSkinned Mesh RendererとMesh Filterに対応しています。

なお、このエフェクトはアクセサリを始めとする小物類に使用することを想定して作成しているため、変形するメッシュには対応していません。

## Installation

このアセットはUnity Package Manager (UPM)を使用してインストールできます。

インストールに必要なパッケージは以下の通りです。

UPMを使用してインストールすることで自動的にインストールされます。

- Universal RP
- Visual Effect Graph

### From git URL

Window > Package Manager を開いて左上の+マークをクリックすると表示される`Add package from git URL...`
をクリックすると表示される入力欄に`git+ssh://git@github.com/Kuyuri-Iroha/MeshSparklyEffect.git?path=/Assets/MeshSparklyEffect`
と入力することで最新のバージョンをインストールすることができます。

### From local disk (Release)

GitHubのReleaseからMeshSparklyEffect.zipをダウンロードして解凍した後、Window > Package Manager
を開いて左上の+マークをクリックすると表示される`Add package from disk...`から解凍したフォルダを選択することでインストールできます。

## 使い方

Project SettingsのGraphicsでScriptable Render Pipeline SettingsにUniversal Render Pipeline Assetが正しくセットされていることを確認した後、
GameObjectにMeshSparklyEffect.csを追加することで、その下層にVisual Effectが追加されたGameObjectが生成されて動作します。

詳しくはドキュメントを御覧ください。

[MeshSparklyEffectのドキュメント](./Assets/MeshSparklyEffect/Documentation/MeshSparklyEffect.md)

## Unity Version & Dependencies

開発バージョン：2020.3.9

- Universal RP
- Visual Effect Graph
