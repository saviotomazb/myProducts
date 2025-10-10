async function fetchWithRefresh(url, options = {}) {
    options.credentials = 'include';

    let response = await fetch(url, options);

    if (response.status === 401) {
        const refreshed = await refreshToken();
        if (refreshed) {
            response = await fetch(url, options);
        }
        else {
            return response;
        }
    }

    return response;
}

async function refreshToken() {
    try {
        const response = await fetch('/Account/RefreshToken', {
            method: 'POST',
            credentials: 'include'
        });

        if (!response.ok) {
            console.warn('Não foi possível renover o token. Redirecionando para o login.');
            window.location.href = '/Account/Login';
            return false;
        }

        const data = await response.json();
        console.log('Refresh Token', data.message);
        return true;
    } catch (error) {
        console.error('Erro ao tentar renovar o token:', error);
        window.location.href = '/Account/Login';
        return false;
    }
}