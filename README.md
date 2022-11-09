# file-service-sample

With the first version, we provide bear minimal capabilities for customers who needs to support the following user stories.
The suggested architecture, detailed shortly, also calls for an audit or scanning capabilities that would ensure the uploaded files are safe before moving them to the secured area, as all files uploaded should be treated as un-safe.

This solution is an end-2-end microservice sample. Once deployed it will create the required Azure resources, configure them and output a URL. The solution focus on a single tenant approach, where all services are under the same tenant.
For authentication/authorization the client credentials [flow] (https://learn.microsoft.com/en-us/azure/active-directory/develop/v2-oauth2-client-creds-grant-flow) was selected.

> Notes:

- The service validating file content is not implemented.

- Authentication and Authorization flow are initial provisioned with minimal validation, addtional role assignment or any other means for authrization can be added by you.

- Network security is out-of-scope for the initial version.

## User stories

As a service provider, I need my customers to uploaded content in a secured, easy to maintain micro-service exposed as an API, so that I can process the content uploaded and perform an action on it.

As a service provider, I would like to enable download of specific files for authorized devices or humans, so that they could download the content directly from storage.

As a service provider, I would like to offer my customers ability to see what files they have already uploaded, so that they can download it when needed with time restricted SaS.

## Design Approach

The following guidelines were considered for the solution approach:

- Time, role and IP based authorization
- Microservice

> Note, the IP restriction is provided as an ability, it does provide minimal layer of security, limiting the reuse of the same SaS by multiple users, for machine2machine communication, it would be better to authenticate the requests and have the time restriction shorter, for example having it as 30 seconds or less.

Here is the draft architecture created:
![art](./images/hlapproach.png)

### Components

1. Container App - create SaS tokens and containers, it also provide SaS for given file with in a given container.

2. Storage Account - DMZ, all content is considered unsafe

3. Container App - verify content and move it to verified storage

4. Verified storage, content is assumed to be verified and has minimal or no threat to the organization

5. Container Registry - holds the container app images

6. Azure KeyVault - holds connection strings and other secured configuration

Addtional resource is the app registration used to govern access, this resource is not created by the bicep code, rather used by it. You will need to create and configure it via the azure portal.

### Getting started - Using Bicep scripts

> Note: Bicep does not create the resource group, so make sure to create one before you start.
Using the following steps you can spin up an entire environment:

Active Directory:
An app registration is required to enable access control. At the time this sample was created there is no support by bicep for this, therefore the suggestion is to use manual steps. Create an app registration, here is a [guide] (https://learn.microsoft.com/en-us/azure/active-directory/develop/scenario-protected-web-api-app-registration) on how to do it. The guide walk you through how to create the app, add scopes. The information you would require from this step would be used to populate the ```param.json``` file. You would need:

- TenantId

- ClientId

- Domain

Once you have these (resource group + app registration) you can follow these steps, which assume you created a resource group named `fs-test-bicep`.

1. Clone the repo

2. Navigate to the ```deploy/bicep``` directory

3. Modify the param.json file to reflect your individual settings

4. Confirm you have are logged in, you can run ```az login```

5. Run

```azurecli
az deployment group create --resource-group fs-test-bicep --template-file main.bicep --parameters @param.json
```

Once you have the environment deployed, check the fqdn of the newly created container app, for both options listed here, you will need to add the ```/swagger``` suffix to get to the exposed apis.

- As part of the deployment output, you can search for 'fqdn' it will have the deployed container app url.

- Another option is to copy it from the portal, you can find it in the overview blade of the container app.

This 'vanilla' version is your starting point, part of this sample, you can also leverage the github actions provided. There are few steps required to be performed on your github repo to enable it to work with your subscription & resource group. There are few online guides that would walk you through this task, here is an [example] (https://learn.microsoft.com/en-us/azure/developer/github/connect-from-azure?tabs=azure-portal%2Clinux).


#### Working with this sample

The initial provisioning is taking an image from this repo (with the latest tag). Part of the sample also include two github actions, that builds and deploy the newly created image to the container app enviorment. There are few manual steps that are required to be done on your cloned repository allowing it to make changes to your Azure resources. There are several guides on how to do it, here is [one] (https://learn.microsoft.com/en-us/azure/container-apps/dapr-github-actions?tabs=bash)

### Flow

#### Authenticate - getting an access token

1. Caller<sup>1</sup> makes a call to the authentication service (in this example AAD) for an access token. This flow is called client credentials flow.

2. Caller can continue to call the microservice with the token aquired.

#### Calling the APIs

1. Caller<sup>1</sup> make a call to obtain a SaS token for a container<sup>2</sup>

2. ContainerApp, extracts the caller IP, creates the token, and return it to the caller

3. Caller uses the token to upload new file to the container in the DMZ storage

4. Second container app, validates the file uploaded is valid/clean and moves it to a verified storage account

<sup>1</sup> -  A caller is an authorized application, user or any other identity, the project has few sample clients that are able to leverage the provided APIs, it is an implementation decision which client to use.

<sup>2</sup> - There are two options, either the caller provided an existing container name, or he can create a new one.

## Main Design Considerations

- Secured, the code must hosted in vnet enabled compute.
- Your storage must not be publicly exposed.
- All uploads are considered unsafe unless verified.
- Customer uploads must land on DMZ storage, with minimal, automatic clean up.
- Use .net6 as programing language
- Use Container Apps as the compute service

## File Server

We provide a few end-points, if the client did not create yet a designated container, he can call the `api/SaSToken` this will create a container with GUID as the name, and return a SaS token for that container. It is expected that the client will reuse this container in any consecutive calls. It will be implemented a validation for this.

### SaSToken

Provides SaS token, based on the time restriction defined in configuration. We created default implementation that also allows for IP restriction based on the caller IP.

#### Get

Create a container and respond with the SaS token with permissions to create new blobs.

#### Get by Container

Gets a SaS token for given container with permissions to create new blobs.

#### Get by Container/File

Gets a read SaS token for the provided container and file path.

### Files

#### Get Container Files

Return the list of files within a given container.

### Request

The request can contain the designated container name, or if one is not passed, one would be created.

Here are two examples, the first is using a created container name, the second is requesting to create one.

```curl
curl -X 'GET' \
  'https://fileserver.grayriver-46b04276.northeurope.azurecontainerapps.io/api/SaSToken/f7b969a1-e800-424a-b9c1-ab6f94e49b6d' \
  -H 'accept: text/plain'
```

```curl
curl -X 'GET' \
  'https://fileserver.grayriver-46b04276.northeurope.azurecontainerapps.io/api/SaSToken' \
  -H 'accept: text/plain'
```

#### test.rest

As part of this repo, I've included the [rest.test](./clients/rest/test.rest) to help test uploading content to the DMZ storage.
You will need to create an `.env` file with these entries, both are provided with in the response of the Token API.

```.env
sas_base_url=<SasTokenBaseUri>
sas_sig=<SasTokenSig>

```

### Response

As part of the response, the full URI is provided together with the captured IP, the container name and also the breakdown of the URI which might be needed for few clients.

Here is a sample response:

```json
{
    "ContainerName":"xxx969a1-e800-424a-b9c1-ab6f94e49xxx",
    "RemoteIp":"XX.XXX.XX.XXX",
    "RequestStatus":"Success",
    "SaSUri":"https://xxxxxxx.blob.core.windows.net/f7b969a1-e800-424a-b9c1-ab6f94e49b6d?sv=2021-08-06&se=2022-09-05T14%3A17%3A51Z&sip=xx.xxx.xx.xxx&sr=c&sp=racwdxyltmei&sig=XXXXXXX",
    "SasTokenBaseUri":"https://xxxxxxx.blob.core.windows.net/f7b969a1-e800-424a-b9c1-ab6f94e49b6d","SasTokenSig":"sv=2021-08-06&se=2022-09-05T08%3A24%3A10Z&sip=xx.xxx.xx.xxx&sr=c&sp=racwdxyltmei&sig=XXXXXXX"
}
```
